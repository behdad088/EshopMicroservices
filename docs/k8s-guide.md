# EshopMicroservices — Kubernetes Guide

> I thought its a good idea to write a little bit about the k8s setup for this project. I try to explain the rationale behind the structure and configuration choices, so it can be a reference for anyone looking to deploy a similar stack on Kubernetes.

---

## Table of Contents

1. [Why Kubernetes?](#1-why-kubernetes)
2. [What We Are Deploying](#2-what-we-are-deploying)
3. [Directory Structure](#3-directory-structure)
4. [Core Infrastructure (00-core)](#4-core-infrastructure-00-core)
   - Namespace
   - RBAC — ServiceAccounts, Roles, RoleBindings
   - Secrets vs ConfigMaps
   - Certificate Management
5. [Databases (01-databases)](#5-databases-01-databases)
   - StatefulSet vs Deployment
   - Why Two Services Per Database?
   - PersistentVolumeClaims
6. [Observability Stack (02-observability)](#6-observability-stack-02-observability)
7. [Mail — Mailpit (03-mail)](#7-mail--mailpit-03-mail)
8. [Microservices (04-services)](#8-microservices-04-services)
   - Deployment anatomy
   - Init containers
   - Health probes
   - Resource requests and limits
   - Discount gRPC — two protocols, two ports
   - HorizontalPodAutoscaler
   - NetworkPolicy
   - PodDisruptionBudget
9. [Ingress (05-ingress)](#9-ingress-05-ingress)
10. [Kustomize — Base & Overlays](#10-kustomize--base--overlays)
11. [Helper Scripts](#11-helper-scripts)
12. [Day-to-Day Workflow](#12-day-to-day-workflow)

---

## 1. Why Kubernetes?

Docker Compose is great for running the stack locally on a single machine. Kubernetes takes over when you need resilience, scalability, and a path to production.

| Problem | Docker Compose                         | Kubernetes |
|---|----------------------------------------|---|
| A container crashes | Stays down until you restart it        | Automatically restarted by kubelet |
| High traffic on catalog-api | Manual, restart with more containers   | HPA scales replicas based on CPU automatically |
| Deploy a new version with zero downtime | Brief downtime during replace          | Rolling update: new pod up before old one stops |
| Drain a node for maintenance | N/A                                    | PodDisruptionBudget guarantees at least 1 pod stays alive |
| Isolate credentials from config | Single .env file mixed with all config | Secrets and ConfigMaps are distinct, RBAC-controlled resources |
| Restrict network traffic | All containers on same bridge network  | NetworkPolicy enforces allow-list between pods |

> **Key Takeaway:** Kubernetes is not just a "fancier Docker Compose". It is a control plane that continuously reconciles your declared desired state with the actual running state of your system.

---

## 2. What We Are Deploying

The full EshopMicroservices stack running in Kubernetes consists of **148 resources** across these categories:

| Category | Components | Count |
|---|---|---|
| Microservices | catalog-api, basket-api, discount-grpc, identity-api, order-command-api, order-query-api, order-query-eventprocessor, api-gateway | 8 Deployments |
| Databases | catalog-db, basket-db, identity-db, order-command-db, order-query-db (PostgreSQL 16), Redis, RabbitMQ | 7 StatefulSets |
| Observability | Elasticsearch, Kibana, OTEL Collector, Jaeger, Prometheus, Loki, Grafana | 1 StatefulSet + 6 Deployments |
| Mail | Mailpit (SMTP testing) | 1 Deployment |
| Networking | nginx Ingress rules exposing 7 external hosts | 7 Ingress resources |
| Security | ServiceAccounts, Roles, RoleBindings, NetworkPolicies | 10 + 2 + 9 + 20 |

```
                     ┌──────────────────────────────┐
                     │      Browser / API Client    │
                     └──────────────┬───────────────┘
                                    │ HTTP  *.eshop.local
                ┌───────────────────▼───────────────────────┐
                │          nginx Ingress Controller         │
                │  api · identity · grafana · kibana · ...  │
                └──────────────┬────────────────────────────┘
                               │ HTTPS (port 8081)
                ┌──────────────▼────────────────────────────┐
                │              API Gateway (YARP)           │
                └──┬────────┬──────────┬────────┬───────────┘
                   │        │          │        │
           catalog-api  basket-api  order-*  identity-api
               │            │  ──gRPC──▶discount-grpc
           catalog-db   basket-db
                      redis        rabbitmq → order-query-eventprocessor
                                                    │
                                              order-query-db
```

---

## 3. Directory Structure

The entire k8s configuration lives in the `k8s/` folder at the repo root. It follows a layered structure: base resources first, then environment-specific overlays on top.

```
k8s/
├── base/                          # Canonical resource definitions
│   ├── 00-core/                   # Namespace, RBAC, Secrets, ConfigMaps
│   │   ├── namespace.yaml
│   │   ├── rbac.yaml
│   │   ├── configmaps/            # One ConfigMap per service
│   │   ├── secrets/               # DB passwords, certs, RabbitMQ creds
│   │   ├── certs/                 # PFX files auto-copied by generate-certs.sh
│   │   └── kustomization.yaml
│   ├── 01-databases/              # PostgreSQL ×5, Redis, RabbitMQ
│   │   ├── catalog-db/            # statefulset.yaml + service.yaml
│   │   ├── basket-db/
│   │   ├── identity-db/
│   │   ├── order-command-db/
│   │   ├── order-query-db/
│   │   ├── redis/
│   │   ├── rabbitmq/
│   │   └── kustomization.yaml
│   ├── 02-observability/          # Full observability stack
│   │   ├── elasticsearch/
│   │   ├── kibana/
│   │   ├── otel-collector/
│   │   ├── jaeger/
│   │   ├── prometheus/
│   │   ├── loki/
│   │   ├── grafana/
│   │   └── kustomization.yaml
│   ├── 03-mail/                   # Mailpit
│   ├── 04-services/               # Microservice Deployments, HPAs, Policies
│   │   ├── catalog-api/           # deployment.yaml · service.yaml · hpa.yaml
│   │   ├── basket-api/
│   │   ├── discount-grpc/
│   │   ├── identity-api/
│   │   ├── order-command-api/
│   │   ├── order-query-api/
│   │   ├── order-query-eventprocessor/
│   │   ├── api-gateway/
│   │   ├── network-policies.yaml
│   │   ├── pod-disruption-budgets.yaml
│   │   └── kustomization.yaml
│   ├── 05-ingress/                # nginx Ingress rules for *.eshop.local
│   └── kustomization.yaml         # Root — references 00-05 in order
├── overlays/
│   ├── local/                     # minikube / Docker Desktop / OrbStack
│   │   ├── kustomization.yaml
│   │   └── patches/
│   │       ├── resource-limits-patch.yaml   # Lighter Elasticsearch for dev
│   │       ├── storage-class-patch.yaml
│   │       └── single-replica-patch.yaml
│   └── production/                # Placeholder for cloud deployments
└── scripts/
    ├── apply.sh                   # Deploy the full stack
    ├── teardown.sh                # Remove the stack (optionally wipe data)
    ├── create-cert-secrets.sh     # Load PFX certs into K8s Secrets
    └── hosts-setup.sh             # Print /etc/hosts entries for ingress
```

> **Why numbered directories?** The numbers (00, 01, 02…) express deployment order. Core resources (namespace, RBAC, secrets) must exist before databases, which must exist before the services that depend on them. Kustomize applies them in the order listed in `kustomization.yaml`.

> **production?** this is a placeholder for future cloud-specific configuration (e.g. GKE with Cloud SQL, EKS with RDS, AKS with Azure Database). The base configuration is designed to be cloud-agnostic, so the production overlay may be minimal. I didn't want to test it with actual cloud providers during development, so I focused on a local setup that works on any machine with Docker Desktop.
---

## 4. Core Infrastructure (00-core)

### 4.1 Namespace

A Kubernetes **namespace** is a logical isolation boundary. Everything in this project lives in one namespace: `eshop-microservices`.

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: eshop-microservices
```

**Why a [namespace](https://kubernetes.io/docs/concepts/overview/working-with-objects/namespaces/)?**
- Isolates all resources from other workloads on the cluster
- [RBAC](https://kubernetes.io/docs/reference/access-authn-authz/rbac/) rules can be scoped to this namespace only; no accidental access to other namespaces
- Easy to nuke everything: `kubectl delete namespace eshop-microservices`
- Resource quotas can be applied per-namespace in production. for example, you can limit how much cluster resources a particular namespace is allowed to consume.

A small [ResourceQuota](https://kubernetes.io/docs/concepts/policy/resource-quotas/) example for the `eshop-microservices` namespace:
```
apiVersion: v1
kind: ResourceQuota
metadata:
  name: frontend-quota
  namespace: eshop-microservices
spec:
  hard:
    pods: "10"
    requests.cpu: "4"
    requests.memory: 8Gi
    limits.cpu: "8"
    limits.memory: 16Gi
```

---

### 4.2 RBAC — ServiceAccounts, Roles, RoleBindings

**RBAC** (Role-Based Access Control) controls what each pod is allowed to *do* inside the cluster; specifically what Kubernetes API calls it can make. By default, pods run as the `default` service account which has broad permissions. We give each service its own minimal-permission identity.

```
ServiceAccount (sa-catalog-api)
      │
      └── RoleBinding (rb-catalog-api)
                │
                └── Role (role-app-readonly)
                        rules:
                        - get on configmaps
                        - get on secrets
```

#### The three RBAC objects

| Object | What it is | Analogy |
|---|---|---|
| `ServiceAccount` | An identity assigned to pods | A user account |
| `RoleBinding` | Assigns a Role to a ServiceAccount | Giving someone that job title |
| `Role` | A named set of permissions within a namespace | A job title with defined permissions |

#### What we created

- **10 ServiceAccounts** — one per service group (`sa-databases`, `sa-observability`) and one per microservice
- **2 Roles**:
  - `role-app-readonly` — microservices can `get` their ConfigMap and Secret
  - `role-observability` — Prometheus can `get/list/watch` Pods and Endpoints for service discovery
- **9 RoleBindings** — each microservice SA bound to `role-app-readonly`

> **Principle of Least Privilege:** No ServiceAccount has `create`, `update`, or `delete` permissions. No ClusterRole is granted; permissions are namespace-scoped only. If a pod is compromised, the blast radius is limited.

---

### 4.3 [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/) vs [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/)

Both are key-value stores that inject configuration into pods. The difference is sensitivity:

| | ConfigMap | Secret |
|---|---|---|
| Stores | Non-sensitive config (URLs, ports, feature flags) | Passwords, tokens, certificates |
| Stored as | Plain text | Base64-encoded |
| Example | `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317` | `POSTGRES_PASSWORD=cG9zdGdyZXM=` |

#### How a microservice gets its config

```yaml
# 1. Non-sensitive config injected from ConfigMap
envFrom:
  - configMapRef:
      name: catalog-api-cm

# 2. Sensitive values injected from Secrets
env:
  - name: DB_PASSWORD
    valueFrom:
      secretKeyRef:
        name: catalog-db-secret
        key: POSTGRES_PASSWORD

# 3. Connection string assembled using K8s env-var interpolation
  - name: ConnectionStrings__Database
    value: "Server=catalog-db;Port=5432;Database=CatalogDb;User Id=$(DB_USER);Password=$(DB_PASSWORD)"
```

> **Why not put the full connection string in a Secret?** The host, port, and database name are not sensitive; they are the same in every environment. Only the password changes. Keeping them in a ConfigMap makes it easy to see the topology without managing secrets unnecessarily.

---

### 4.4 Certificate Management

Each ASP.NET Core service uses Kestrel's built-in HTTPS with a PFX certificate. In Kubernetes, certificates live as **Secrets**.

#### The certificate flow

```
1. src/certificates/generate-certs.sh
     ↓ generates CA + 8 PFX files
     ↓ auto-copies to k8s/base/00-core/certs/

2. k8s/scripts/create-cert-secrets.sh
     ↓ reads each .pfx
     ↓ creates Kubernetes Secret: cert-catalog-api, cert-basket-api, ...
     ↓ creates ConfigMap: cert-ca (the public CA certificate)

3. Each Deployment mounts a projected volume:
     /app/certificates/catalog.pfx   ← from Secret
     /app/certificates/ca.crt        ← from ConfigMap
```

```yaml
# Projected volume in every microservice Deployment
volumes:
  - name: certs
    projected:
      sources:
        - secret:
            name: cert-catalog-api    # private PFX
        - configMap:
            name: cert-ca             # public CA cert for trust chain
```

> **Projected volumes** merge multiple sources (secrets, configmaps) into a single directory mount. This is why both the PFX and the CA cert appear in the same `/app/certificates/` folder inside the container.

---

## 5. Databases (01-databases)

### 5.1 [StatefulSet](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/) vs [Deployment](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)

Databases have state; they write data to disk. Kubernetes provides two workload types:

| | Deployment | StatefulSet |
|---|---|---|
| Pod naming | Random suffix: `catalog-api-6f9b-xkp2z` | Ordered, stable: `catalog-db-0`, `catalog-db-1` |
| Storage | Shared or ephemeral | Each pod gets its own dedicated PVC |
| Startup/shutdown | All pods start/stop simultaneously | Ordered: 0 before 1, 1 before 2 |
| Use for | Stateless services (APIs, proxies) | Stateful services (databases, queues) |

All 7 data stores (5× PostgreSQL, Redis, RabbitMQ) are StatefulSets. Elasticsearch is also a StatefulSet.

---

### 5.2 Why Two Services Per Database?

Every database StatefulSet has **two Kubernetes Services**. This is a standard Kubernetes pattern:

```
catalog-db-0 (pod)
     │
     ├── catalog-db-headless  (clusterIP: None)
     │      DNS: catalog-db-0.catalog-db-headless.eshop-microservices.svc.cluster.local
     │      Used by: StatefulSet controller (required)
     │      Behaviour: Returns the pod IP directly. No load balancing.
     │
     └── catalog-db  (ClusterIP)
            DNS: catalog-db  (short form within namespace)
            Used by: catalog-api connection string
            Behaviour: Load-balanced → routes to ready pod
```
K8s uses headless DNS internally for pod identity, ordered startup, and PVC binding. This happens whether or not any app ever uses these DNS names. It's not optional.

| Service | clusterIP | Who uses it | Why |
|---|---|---|---|
| `catalog-db-headless` | `None` | K8s StatefulSet controller | Gives each pod a stable unique DNS name. Required by `spec.serviceName`. |
| `catalog-db` | Assigned by K8s | `catalog-api` connection string | Stable address for apps. Handles pod failover transparently. |

> **When does the headless service become critical?** With a single replica the headless service isn’t technically necessary. But if you scaled to 2 replicas with PostgreSQL streaming replication, the primary would be at `catalog-db-0.catalog-db-headless` and the replica at `catalog-db-1.catalog-db-headless`; and the application (if necessary) could choose which to connect to.

---

### 5.3 [PersistentVolumeClaims](https://kubernetes.io/docs/concepts/storage/persistent-volumes/)

StatefulSets use `volumeClaimTemplates`; a template that automatically creates one PVC per pod:

```yaml
volumeClaimTemplates:
  - metadata:
      name: data
    spec:
      accessModes: [ReadWriteOnce]   # Only one pod can mount this at a time
      storageClassName: local-path   # Patched per environment by overlay
      resources:
        requests:
          storage: 1Gi
```

| Term | Meaning |
|---|---|
| `PersistentVolume (PV)` | Actual storage provisioned on the cluster (a disk) |
| `PersistentVolumeClaim (PVC)` | A request for storage by a pod. K8s binds PVC → PV automatically. |
| `StorageClass` | Defines how storage is provisioned. `local-path` (OrbStack/minikube), `pd-ssd` (GKE), `gp3` (EKS). |
| `ReadWriteOnce` | Only one node can mount this volume at a time. Fine for single-replica databases. |

> **PVCs survive StatefulSet deletion.** When you run `kubectl delete -k k8s/overlays/local`, all StatefulSets are deleted but their PVCs remain. Your data is preserved across redeployments. Add `kubectl delete pvc --all -n eshop-microservices` only when you want a clean slate.

Note: Use volumeClaimTemplates in StatefulSets when each replica needs its own isolated storage (databases). Use a standalone PVC + Deployment when you have a single-replica workload that just needs persistent storage (Prometheus, Loki, Mailpit). if you deployment has more than one replica you need to use ReadWriteMany mode. ReadWriteOnce mode allow only one replica to mount. 

---

## 6. Observability Stack (02-observability)

The observability stack provides three pillars: **metrics**, **logs**, and **traces**.

```
Microservice
    │
    └── OpenTelemetry SDK (in every .NET service)
            │
            ▼ OTLP (gRPC port 4317)
        otel-collector
            │
            ├── Traces ──────────────▶  Jaeger     (jaeger.eshop.local)
            ├── Metrics (Prometheus) ▶  Prometheus  (prometheus.eshop.local)
            │                              └──▶ Grafana  (grafana.eshop.local)
            └── Logs ────────────────▶  Loki
                                           └──▶ Grafana

Microservices also send structured logs to Elasticsearch:
    └──▶ Elasticsearch ──▶ Kibana  (kibana.eshop.local)
```

| Component | Role | Storage |
|---|---|---|
| OTEL Collector | Central telemetry aggregator — receives from all services, fans out to backends | Stateless |
| Jaeger | Distributed tracing UI — shows end-to-end request flows across services | In-memory (stateless) |
| Prometheus | Time-series metrics database — scrapes all service /metrics endpoints | PVC 1Gi |
| Loki | Log aggregation — stores structured logs from OTEL Collector | PVC 1Gi |
| Grafana | Dashboard UI — pre-provisioned with Prometheus, Loki, and Jaeger datasources | Stateless |
| Elasticsearch | Full-text log search engine — receives logs from Serilog in each service | StatefulSet, PVC 2Gi |
| Kibana | Elasticsearch UI — log search and dashboards | Stateless |

#### Why Grafana is stateless

Grafana's datasources and dashboards are *provisioned via a ConfigMap* at startup. Nothing is stored inside the container itself, so it needs no PVC. Delete and recreate it freely without losing any configuration.

```yaml
# grafana/provisioning-cm.yaml — auto-loads datasources on startup
data:
  datasources.yaml: |
    datasources:
      - name: Prometheus
        type: prometheus
        url: http://prometheus:9090
        isDefault: true
      - name: Loki
        type: loki
        url: http://loki:3100
      - name: Jaeger
        type: jaeger
        url: http://jaeger:16686
```

---

## 7. Mail — Mailpit (03-mail)

Mailpit is a local SMTP server and web UI for catching emails during development. The Identity service sends verification emails — Mailpit traps them so they never reach real inboxes.

| Port | Purpose | Access |
|---|---|---|
| 1025 | SMTP — receives emails from identity-api | Internal only |
| 8025 | Web UI — browse captured emails | `http://mailpit.eshop.local` |

---

## 8. Microservices (04-services)

### 8.1 Anatomy of a Microservice Deployment

Every microservice follows the same pattern. Annotated Deployment for catalog-api:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: catalog-api
  namespace: eshop-microservices
spec:
  replicas: 1                        # HPA will scale this automatically
  selector:
    matchLabels:
      app: catalog-api
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1                    # Spin up 1 new pod before removing old one
      maxUnavailable: 0              # This is for zero downtime: never kill pod until new is ready
  template:
    spec:
      serviceAccountName: sa-catalog-api  # Least-privilege identity
      initContainers: # Wait for DB before starting
        - name: wait-for-db
          image: busybox:1.36
          command: [ 'sh', '-c', 'until nc -z catalog-db 5432; do sleep 2; done' ]
      containers:
        - name: catalog-api
          envFrom: # All non-sensitive config from ConfigMap
            - configMapRef:
                name: catalog-api-cm
          env: # Sensitive values from Secrets
            - name: DB_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: catalog-db-secret
                  key: POSTGRES_PASSWORD
          livenessProbe: # K8s restarts pod if this fails
            httpGet: { path: /hc, port: 8080 }
          readinessProbe: # K8s stops routing traffic if this fails
            httpGet: { path: /hc, port: 8080 }
          resources:
            requests: { cpu: 100m, memory: 128Mi }
            limits: { cpu: 500m, memory: 512Mi }
          volumeMounts:
            - name: certs
              mountPath: /app/certificates
              readOnly: true
```

---

### 8.2 Init Containers

An **init container** runs to completion before the main container starts. We use them to make each microservice wait until its database is accepting connections:

```yaml
initContainers:
  - name: wait-for-db
    image: busybox:1.36
    command:
      - sh
      - -c
      - 'until nc -z catalog-db 5432; do echo waiting for catalog-db; sleep 2; done'
```

Without this, the service would start, fail to connect to the database, crash, and restart repeatedly (CrashLoopBackOff). Init containers solve the startup ordering problem elegantly.

> **Why not use `depends_on` like Docker Compose?** Kubernetes has no native equivalent of `depends_on`. Init containers are the idiomatic solution.

---

### 8.3 Health Probes

Every service exposes a `/hc` endpoint via ASP.NET Core Health Checks. Kubernetes uses two probe types:

| Probe | What it does | On failure |
|---|---|---|
| **livenessProbe** | Is the process alive? Checks if it is stuck or deadlocked. | Pod is killed and restarted |
| **readinessProbe** | Is the pod ready to receive traffic? Checks if dependencies are healthy. | Pod removed from Service endpoints (no traffic sent) |

Both probes use `GET /hc port 8080`. The key difference: **liveness restarts the pod; readiness just stops traffic to it.**

---

### 8.4 Resource Requests and Limits

```yaml
resources:
  requests: { cpu: 100m, memory: 128Mi }   # Guaranteed minimum
  limits:   { cpu: 500m, memory: 512Mi }   # Hard maximum
```

| Term | Meaning |
|---|---|
| `100m` CPU | 100 millicores = 0.1 of one CPU core |
| `requests` | K8s scheduler uses this to decide which node can fit the pod. The pod is guaranteed at least this much. |
| `limits` | Pod is killed (OOMKilled) if it exceeds memory limit. CPU is throttled at the limit. |

---

### 8.5 Discount gRPC — Two Protocols, Two Ports

The Discount service speaks **gRPC** (not HTTP), and needs two ports because two different callers use it differently:

```
basket-api  ──gRPC plaintext (h2c)──▶  discount-grpc:8080
api-gateway ──HTTPS (YARP proxy)──▶   discount-grpc:8081
```

```yaml
# Service exposes both protocols
ports:
  - port: 8080
    name: grpc
    appProtocol: kubernetes.io/h2c    # HTTP/2 cleartext — required for gRPC
  - port: 8081
    name: https
```

Liveness/readiness probes use the native Kubernetes gRPC probe type (K8s 1.24+):

```yaml
livenessProbe:
  grpc:
    port: 8080    # gRPC health protocol on the plaintext port
```

---

### 8.6 [HorizontalPodAutoscaler](https://kubernetes.io/docs/concepts/workloads/autoscaling/horizontal-pod-autoscale/)

An HPA automatically adjusts the number of pod replicas based on CPU usage:

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
spec:
  scaleTargetRef:
    kind: Deployment
    name: catalog-api
  minReplicas: 1
  maxReplicas: 5
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70   # Scale up when avg CPU > 70%
```

HPAs are applied to: `catalog-api` (max 5), `basket-api` (max 5), `order-command-api` (max 3), `order-query-api` (max 3), `api-gateway` (max 3).

**Not applied to:** `discount-grpc` (low-traffic internal), `order-query-eventprocessor` (must have exactly one consumer per queue).

---

### 8.7 [NetworkPolicy](https://kubernetes.io/docs/concepts/services-networking/network-policies/)

By default, all pods in a namespace can talk to each other freely. NetworkPolicy changes this to an allow-list model.

We start with a **deny-all** default and add explicit allow rules:

```yaml
# Step 1: Deny everything
kind: NetworkPolicy
metadata:
  name: default-deny-ingress
spec:
  podSelector: {}     # Applies to ALL pods in namespace
  policyTypes: [Ingress]

# Step 2: Allow only what is documented
kind: NetworkPolicy
metadata:
  name: netpol-catalog-api
spec:
  podSelector:
    matchLabels: { app: catalog-api }
  egress:
    - to: [{ podSelector: { matchLabels: { app: catalog-db }}}]
      ports: [{ port: 5432 }]
    - to: [{ podSelector: { matchLabels: { app: otel-collector }}}]
      ports: [{ port: 4317 }]
```

> **NetworkPolicy requires a CNI plugin that supports it.** Docker Desktop and OrbStack's default CNI does not enforce NetworkPolicy. In production (GKE, EKS, AKS), network policies are enforced by default.

---

### 8.8 [PodDisruptionBudget](https://kubernetes.io/docs/reference/kubernetes-api/policy/pod-disruption-budget-v1/)

A PDB prevents Kubernetes from evicting too many pods at once during planned maintenance (e.g. node drain for an OS upgrade):

```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
spec:
  minAvailable: 1    # At least 1 pod must stay running during disruption
  selector:
    matchLabels: { app: catalog-api }
```

With `minAvailable: 1` and a single replica, a node drain will *wait* until the pod is rescheduled elsewhere before proceeding. Especially important for database nodes in production.

---

## 9. [Ingress](https://kubernetes.io/docs/concepts/services-networking/ingress/) (05-ingress)

The **Ingress** resource is a layer-7 HTTP router sitting in front of all services and routing requests based on hostname.

```
Browser: GET http://kibana.eshop.local
    │
    ▼
/etc/hosts: kibana.eshop.local → 198.19.249.2
    │
    ▼
nginx Ingress Controller (LoadBalancer on port 80)
    │
    ├── Host: api.eshop.local        ──▶  api-gateway:8081   (HTTPS backend)
    ├── Host: identity.eshop.local   ──▶  identity-api:8081  (HTTPS backend)
    ├── Host: kibana.eshop.local     ──▶  kibana:5601         (HTTP backend)
    ├── Host: grafana.eshop.local    ──▶  grafana:3000        (HTTP backend)
    ├── Host: jaeger.eshop.local     ──▶  jaeger:16686        (HTTP backend)
    ├── Host: prometheus.eshop.local ──▶  prometheus:9090     (HTTP backend)
    └── Host: mailpit.eshop.local    ──▶  mailpit:8025        (HTTP backend)
```

#### HTTPS backends

The API Gateway and Identity API terminate TLS themselves using Kestrel. nginx needs to be told to connect to them over HTTPS:

```yaml
metadata:
  annotations:
    nginx.ingress.kubernetes.io/backend-protocol: "HTTPS"
    nginx.ingress.kubernetes.io/proxy-ssl-verify: "false"  # Self-signed CA
```

The other services (Kibana, Grafana, etc.) are plain HTTP; no annotation needed.

---

## 10. Kustomize — Base & Overlays

Kustomize solves a real problem: you want the same application running in different environments with slightly different configuration, but you don't want to duplicate 88 YAML files.

### 10.1 Why Kustomize instead of Helm?

| | Kustomize | Helm |
|---|---|---|
| Templating | No templating — patch existing YAML | Go template language in all YAML files |
| Built into kubectl | Yes — `kubectl apply -k` | No — requires separate installation |
| Learning curve | Low — you just write plain YAML | Higher — template syntax, values.yaml, chart structure |
| Base resources | Always valid YAML, readable at a glance | Filled with `{{ .Values.something }}` |
| Best for | Small/medium projects, plain YAML fans | Complex packages shared across many teams |

### 10.2 The base kustomization

```yaml
# k8s/base/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
resources:
  - 00-core        # Applied first — namespace must exist before everything else
  - 01-databases   # Applied second — databases before services that depend on them
  - 02-observability
  - 03-mail
  - 04-services
  - 05-ingress
```

### 10.3 The local overlay

```yaml
# k8s/overlays/local/kustomization.yaml
resources:
  - ../../base

patches:
  # Patch 1: Change storageClass to local-path for all StatefulSets
  - patch: |-
      - op: replace
        path: /spec/volumeClaimTemplates/0/spec/storageClassName
        value: local-path
    target: { kind: StatefulSet }

  # Patch 2: Change storageClass to local-path for standalone PVCs
  - patch: |-
      - op: replace
        path: /spec/storageClassName
        value: local-path
    target: { kind: PersistentVolumeClaim }

  # Patch 3: Reduce Elasticsearch memory for local dev machines
  - path: patches/resource-limits-patch.yaml
    target: { kind: StatefulSet, name: elasticsearch }

images:           # Override image tags per environment
  - name: catalog.api
    newName: catalog.api
    newTag: latest
```

> **The power of Kustomize patches:** The two inline JSON patches (`op: replace`) change the storage class for *all* StatefulSets and PVCs in a single declaration. Without Kustomize, you would have to edit 10+ files.

---

## 11. Helper Scripts

| Script | What it does | When to run |
|---|---|---|
| `generate-certs.sh` | Creates CA + 8 PFX certificates with K8s DNS SANs, auto-copies to `k8s/base/00-core/certs/` | Once (re-run when certs expire after 365 days) |
| `create-cert-secrets.sh` | Reads PFX files and creates Kubernetes Secrets + `cert-ca` ConfigMap | After generate-certs.sh, or after a full cluster reset |
| `apply.sh` | Checks cluster, runs cert script if secrets are missing, then `kubectl apply -k overlays/local` | Every deploy |
| `teardown.sh` | Runs `kubectl delete -k overlays/local`. With `--wipe-data` also deletes all PVCs. | When removing the stack |
| `hosts-setup.sh` | Detects the ingress LoadBalancer IP, prints 7 `/etc/hosts` entries | Once per machine (after first deploy) |

---

## 12. Day-to-Day Workflow

### First-time setup

```bash
# 1. Enable Kubernetes in Docker Desktop, then install nginx ingress controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.5/deploy/static/provider/cloud/deploy.yaml

# 2. Build images
cd src && docker compose build && cd ..

# 3. Generate certs (writes to k8s/base/00-core/certs/ automatically)
mkdir -p k8s/base/00-core/certs
cd src/certificates && bash generate-certs.sh && cd ../..

# 4. Deploy
bash k8s/scripts/apply.sh local

# 5. Set up /etc/hosts (once per machine)
#    The IP is stable across eshop redeployments — only redo this after a full cluster reset.
bash k8s/scripts/hosts-setup.sh | sudo tee -a /etc/hosts
```

### Daily workflow

```bash
# After changing code, rebuild and redeploy:
cd src && docker compose build && cd ..
bash k8s/scripts/apply.sh local

# Tear down (data preserved in PVCs):
bash k8s/scripts/teardown.sh local

# Tear down + wipe all data:
bash k8s/scripts/teardown.sh local --wipe-data

# Watch pod status:
kubectl get pods -n eshop-microservices -w

# See logs for a service:
kubectl logs -n eshop-microservices -l app=catalog-api --follow

# Describe a crashing pod:
kubectl describe pod -n eshop-microservices <pod-name>
```

### URLs after deployment

| URL | What you see |
|---|---|
| `http://api.eshop.local` | API Gateway — entry point for all API calls |
| `http://identity.eshop.local` | Identity Server — OIDC login / token endpoints |
| `http://grafana.eshop.local` | Grafana — metrics + logs dashboards (anonymous admin) |
| `http://kibana.eshop.local` | Kibana — Elasticsearch log search UI |
| `http://jaeger.eshop.local` | Jaeger — distributed trace explorer |
| `http://prometheus.eshop.local` | Prometheus — raw metrics query UI |
| `http://mailpit.eshop.local` | Mailpit — captured emails from Identity service |

---

*EshopMicroservices Kubernetes Guide — Behdad Kardgar*
