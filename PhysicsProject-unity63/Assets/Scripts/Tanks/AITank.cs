using UnityEngine;
using System.Collections.Generic;

public class AITank : MonoBehaviour
{
    public enum AIBehavior
    {
        Patrol,
        Aggressive,
        Defensive
    }

    [Header("Comportamento")]
    public AIBehavior behavior;

    [Header("Tiro")]
    public GameObject bulletPrefab;
    public GameObject bulletSpawn;
    public GameObject enemy;
    public Transform cannon;

    public float rotationSpeed = 2.0f;
    public float moveSpeed = 3.0f;

    public float bulletSpeed = 15.0f;
    public float fireRate = 1.5f;
    public float attackRange = 10.0f;

    [Header("Distância de perseguição")]
    public float distanciaParada = 8.0f;

    [Header("FOV")]
    public float viewDistance = 30.0f;
    public float fovAngle = 90.0f;

    [Header("Obstáculos")]
    public LayerMask obstacleMask;

    [Header("A*")]
    public AStarPathfinding pathfinding;
    public float pathRecalculationTime = 0.5f;

    private List<AStarNode> caminho;
    private int indiceNodeAtual;
    private float nextPathCalculation;

    [Header("Memória do alvo")]
    public float tempoMemoria = 3.0f;

    public Vector3 ultimaPosicaoConhecida;
    public float tempoUltimaDeteccao = -Mathf.Infinity;

    public bool temUltimaPosicaoConhecida;

    [Header("Patrulha")]
    public Transform[] waypoints;

    public int waypointAtual;

    public WaypointRegion regiaoPatrulha;

    [Header("Patrulha Aggressive")]
    public Vector2 tamanhoNovaPatrulha =
        new Vector2(20f, 20f);

    public int quantidadeWaypointsNovaPatrulha = 5;

    public bool agressiveProcurandoUltimaPosicao =
        false;

    public bool regiaoAgressivaDinamica =
        false;

    [Header("Defensive")]
    public WaypointRegion rotaAtual;

    private float nextFireTime;

    void Start()
    {
        if (pathfinding == null)
        {
            pathfinding =
                FindFirstObjectByType<AStarPathfinding>();
        }

        if (behavior == AIBehavior.Patrol)
        {
            EscolherRegiaoInicial();
        }
        else if (behavior == AIBehavior.Aggressive)
        {
            EscolherRegiaoInicial();
        }
        else if (behavior == AIBehavior.Defensive)
        {
            EscolherRotaDefensiva();
        }
    }

    void OnDestroy()
    {
        if (regiaoPatrulha != null)
        {
            regiaoPatrulha.Liberar(this);

            if (regiaoAgressivaDinamica)
            {
                Destroy(
                    regiaoPatrulha.gameObject
                );
            }
        }

        if (rotaAtual != null)
        {
            rotaAtual.Liberar(this);
        }
    }

    // =========================================================
    // VISÃO
    // =========================================================

    public bool PlayerVisivel()
    {
        if (enemy == null)
        {
            return false;
        }

        return PlayerDentroDoFOV() &&
               TemLinhaDeVisao();
    }

    public bool PlayerDentroDoFOV()
    {
        if (enemy == null)
        {
            return false;
        }

        Vector3 direcao =
            enemy.transform.position -
            transform.position;

        float distancia =
            direcao.magnitude;

        if (distancia > viewDistance)
        {
            return false;
        }

        direcao.Normalize();

        float angulo =
            Vector3.Angle(
                transform.forward,
                direcao
            );

        return angulo <=
            fovAngle / 2f;
    }

    public bool TemLinhaDeVisao()
    {
        if (enemy == null)
        {
            return false;
        }

        Vector3 origem =
            transform.position +
            Vector3.up;

        Vector3 destino =
            enemy.transform.position +
            Vector3.up;

        Vector3 direcao =
            destino -
            origem;

        float distancia =
            direcao.magnitude;

        if (Physics.Raycast(
            origem,
            direcao.normalized,
            out RaycastHit hit,
            distancia,
            obstacleMask
        ))
        {
            return false;
        }

        return true;
    }

    // =========================================================
    // MOVIMENTO / A*
    // =========================================================

    public void Parar()
    {
        // O tanque permanece parado
        // enquanto está no AttackState.
    }

    public void SeguirCaminho(
        Vector3 destino
    )
    {
        Vector3 direcao =
            destino -
            transform.position;

        direcao.y = 0f;

        if (direcao.magnitude < 0.05f)
        {
            return;
        }

        Quaternion rotacao =
            Quaternion.LookRotation(
                direcao
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotacao,
                rotationSpeed *
                Time.deltaTime
            );

        transform.position +=
            transform.forward *
            moveSpeed *
            Time.deltaTime;
    }

    public void LimparCaminho()
    {
        caminho = null;
        indiceNodeAtual = 0;
    }

    public void PerseguirDestino(
        Vector3 destino
    )
    {
        if (pathfinding == null)
        {
            return;
        }

        // =====================================================
        // SE O PLAYER ESTÁ EM ÁREA LIVRE
        // =====================================================

        bool destinoEmObstaculo =
            Physics.CheckSphere(
                destino,
                0.5f,
                obstacleMask
            );

        if (!destinoEmObstaculo)
        {
            // Não precisa usar A*.
            // Vai diretamente para o destino.

            LimparCaminho();

            SeguirCaminho(
                destino
            );

            return;
        }

        // =====================================================
        // DESTINO ESTÁ EM OBSTÁCULO
        // =====================================================
        //
        // Usa A* para chegar ao Node livre mais próximo.
        //

        // =====================================================
        // VERIFICA SE PRECISA CALCULAR NOVO CAMINHO
        // =====================================================

        bool precisaCalcularCaminho =
            caminho == null ||
            caminho.Count == 0 ||
            Time.time >= nextPathCalculation ||
            indiceNodeAtual >= caminho.Count;

        if (precisaCalcularCaminho)
        {
            AStarNode startNode =
                GetNearestNode(
                    transform.position
                );

            AStarNode endNode =
                GetNearestNode(
                    destino
                );

            if (
                startNode != null &&
                endNode != null
            )
            {
                caminho =
                    pathfinding.FindPath(
                        startNode,
                        endNode
                    );

                indiceNodeAtual = 0;
            }

            nextPathCalculation =
                Time.time +
                pathRecalculationTime;
        }

        // =====================================================
        // SEM CAMINHO
        // =====================================================

        if (
            caminho == null ||
            caminho.Count == 0
        )
        {
            return;
        }

        // =====================================================
        // SEGUE O NODE ATUAL
        // =====================================================

        if (
            indiceNodeAtual >=
            caminho.Count
        )
        {
            indiceNodeAtual =
                caminho.Count - 1;
        }

        AStarNode nodeAtual =
            caminho[indiceNodeAtual];

        if (nodeAtual == null)
        {
            LimparCaminho();
            return;
        }

        SeguirCaminho(
            nodeAtual.transform.position
        );

        // =====================================================
        // CHEGOU NO NODE
        // =====================================================

        float distancia =
            Vector3.Distance(
                transform.position,
                nodeAtual.transform.position
            );

        if (distancia < 1.2f)
        {
            indiceNodeAtual++;

            if (
                indiceNodeAtual >=
                caminho.Count
            )
            {
                caminho = null;

                indiceNodeAtual = 0;

                nextPathCalculation = 0f;
            }
        }
    }

    AStarNode GetNearestNode(
        Vector3 position
    )
    {
        if (
            pathfinding == null ||
            pathfinding.grid == null
        )
        {
            return null;
        }

        AStarNode melhorNode =
            null;

        float menorDistancia =
            Mathf.Infinity;

        foreach (
            AStarNode node
            in pathfinding.grid
        )
        {
            if (node == null)
            {
                continue;
            }

            // Nunca escolhe Node que está
            // dentro de um obstáculo.
            if (
                node.status ==
                NodeStatus.Obstacle
            )
            {
                continue;
            }

            float distanciaX =
                node.transform.position.x -
                position.x;

            float distanciaZ =
                node.transform.position.z -
                position.z;

            float distancia =
                distanciaX * distanciaX +
                distanciaZ * distanciaZ;

            if (
                distancia <
                menorDistancia
            )
            {
                menorDistancia =
                    distancia;

                melhorNode =
                    node;
            }
        }

        return melhorNode;
    }

    // =========================================================
    // DISTÂNCIA
    // =========================================================

    public float DistanciaDoPlayer()
    {
        if (enemy == null)
        {
            return Mathf.Infinity;
        }

        return Vector3.Distance(
            transform.position,
            enemy.transform.position
        );
    }

    public bool EstaDentroDaDistanciaDeParada()
    {
        return DistanciaDoPlayer() <=
            distanciaParada;
    }

    public bool EstaDentroDoAlcanceDeAtaque()
    {
        return DistanciaDoPlayer() <=
            attackRange;
    }

    // =========================================================
    // MIRA BALÍSTICA
    // =========================================================

    public void MirarCanhao()
    {
        if (
            cannon == null ||
            enemy == null
        )
        {
            return;
        }

        float? angle =
            CalculateAngle(true);

        if (angle != null)
        {
            Vector3 direcaoHorizontal =
                enemy.transform.position -
                transform.position;

            direcaoHorizontal.y = 0f;

            if (
                direcaoHorizontal.sqrMagnitude >
                0.001f
            )
            {
                Quaternion rotacaoHorizontal =
                    Quaternion.LookRotation(
                        direcaoHorizontal.normalized
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        rotacaoHorizontal,
                        rotationSpeed *
                        Time.deltaTime
                    );
            }

            Quaternion rotacaoVertical =
                Quaternion.Euler(
                    360f - (float)angle,
                    0f,
                    0f
                );

            cannon.localRotation =
                Quaternion.Slerp(
                    cannon.localRotation,
                    rotacaoVertical,
                    rotationSpeed *
                    Time.deltaTime
                );
        }
    }

    float? CalculateAngle(bool low)
    {
        float angle = 0.0f;

        Vector3 targetDir =
            enemy.transform.position -
            transform.position;

        float y =
            targetDir.y;

        targetDir.y = 0;

        float x =
            targetDir.magnitude - 1;

        float gravity =
            9.81f;

        float sSqr =
            bulletSpeed *
            bulletSpeed;

        float underTheRoot =
            sSqr * sSqr -
            gravity *
            (
                gravity * x * x +
                2 * y * sSqr
            );

        if (underTheRoot >= 0)
        {
            float root =
                Mathf.Sqrt(
                    underTheRoot
                );

            float highAngle =
                sSqr + root;

            float lowAngle =
                sSqr - root;

            if (low)
            {
                angle =
                    Mathf.Atan2(
                        lowAngle,
                        gravity * x
                    ) *
                    Mathf.Rad2Deg;

                return angle;
            }

            angle =
                Mathf.Atan2(
                    highAngle,
                    gravity * x
                ) *
                Mathf.Rad2Deg;

            return angle;
        }

        return null;
    }

    // =========================================================
    // ATAQUE
    // =========================================================

    public void Atacar()
    {
        if (
            Time.time <
            nextFireTime
        )
        {
            return;
        }

        if (
            bulletPrefab == null ||
            bulletSpawn == null
        )
        {
            return;
        }

        GameObject bala =
            Instantiate(
                bulletPrefab,
                bulletSpawn.transform.position,
                bulletSpawn.transform.rotation
            );

        Rigidbody rb =
            bala.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                bulletSpawn.transform.forward *
                bulletSpeed;
        }

        nextFireTime =
            Time.time +
            fireRate;
    }

    // =========================================================
    // ESCOLHER REGIÃO INICIAL
    // PATROL / AGGRESSIVE
    // =========================================================

    public void EscolherRegiaoInicial()
    {
        WaypointRegion[] regioes =
            FindObjectsByType<WaypointRegion>(
                FindObjectsSortMode.None
            );

        WaypointRegion melhorRegiao =
            null;

        float menorDistancia =
            Mathf.Infinity;

        foreach (
            WaypointRegion regiao
            in regioes
        )
        {
            if (regiao == null)
            {
                continue;
            }

            if (!regiao.EstaLivre())
            {
                continue;
            }

            if (
                regiao.waypoints == null ||
                regiao.waypoints.Length == 0
            )
            {
                continue;
            }

            float distancia =
                Vector3.Distance(
                    transform.position,
                    regiao.transform.position
                );

            if (
                distancia <
                menorDistancia
            )
            {
                menorDistancia =
                    distancia;

                melhorRegiao =
                    regiao;
            }
        }

        if (melhorRegiao != null)
        {
            if (
                melhorRegiao.TentarOcupar(this)
            )
            {
                regiaoPatrulha =
                    melhorRegiao;

                regiaoAgressivaDinamica =
                    false;

                waypoints =
                    melhorRegiao.waypoints;

                waypointAtual =
                    0;

                LimparCaminho();
            }
        }
    }

    // =========================================================
    // DEFENSIVE
    // =========================================================

    public void EscolherRotaDefensiva()
    {
        WaypointRegion[] regioes =
            FindObjectsByType<WaypointRegion>(
                FindObjectsSortMode.None
            );

        WaypointRegion melhorRegiao =
            null;

        float menorDistancia =
            Mathf.Infinity;

        foreach (
            WaypointRegion regiao
            in regioes
        )
        {
            if (regiao == null)
            {
                continue;
            }

            if (!regiao.EstaLivre())
            {
                continue;
            }

            if (
                regiao.waypoints == null ||
                regiao.waypoints.Length == 0
            )
            {
                continue;
            }

            float distancia =
                Vector3.Distance(
                    transform.position,
                    regiao.transform.position
                );

            if (
                distancia <
                menorDistancia
            )
            {
                menorDistancia =
                    distancia;

                melhorRegiao =
                    regiao;
            }
        }

        if (melhorRegiao != null)
        {
            if (
                melhorRegiao.TentarOcupar(this)
            )
            {
                rotaAtual =
                    melhorRegiao;

                waypoints =
                    melhorRegiao.waypoints;

                waypointAtual =
                    0;

                LimparCaminho();
            }
        }
    }

    // =========================================================
    // AGGRESSIVE — NOVA PATRULHA
    // =========================================================

    public void CriarNovaPatrulhaAggressive()
    {
        Vector3 novaPosicao =
            ultimaPosicaoConhecida;

        // Libera a região anterior.
        if (regiaoPatrulha != null)
        {
            bool eraDinamica =
                regiaoAgressivaDinamica;

            regiaoPatrulha.Liberar(this);

            // Regiões dinâmicas antigas são destruídas.
            if (eraDinamica)
            {
                Destroy(
                    regiaoPatrulha.gameObject
                );
            }

            regiaoPatrulha = null;

            regiaoAgressivaDinamica =
                false;
        }

        // Cria uma nova região.
        GameObject novaRegiao =
            new GameObject(
                "Aggressive Patrol Region"
            );

        novaRegiao.transform.position =
            novaPosicao;

        WaypointRegion regiao =
            novaRegiao.AddComponent<
                WaypointRegion
            >();

        regiao.regionSize =
            tamanhoNovaPatrulha;

        regiao.waypointCount =
            quantidadeWaypointsNovaPatrulha;

        regiao.obstacleMask =
            obstacleMask;

        regiao.GenerateWaypoints();

        if (
            !regiao.TentarOcupar(this)
        )
        {
            Destroy(novaRegiao);
            return;
        }

        regiaoPatrulha =
            regiao;

        regiaoAgressivaDinamica =
            true;

        waypoints =
            regiao.waypoints;

        waypointAtual =
            0;

        LimparCaminho();

        nextPathCalculation =
            0f;
    }
}