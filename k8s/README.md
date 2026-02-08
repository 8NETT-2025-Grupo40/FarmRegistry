# Farm Registry API - Kubernetes Deployment

This directory contains the Helm chart for deploying the Farm Registry API to Kubernetes (Amazon EKS).

## 📁 Structure

```
k8s/
├── Chart.yaml                 # Helm chart metadata
├── values.yaml                # Configuration values
└── templates/
    ├── _helpers.tpl          # Template helper functions
    ├── deployment.yaml       # Kubernetes Deployment
    ├── service.yaml          # Kubernetes Service (ClusterIP)
    ├── ingress.yaml          # ALB Ingress
    ├── hpa.yaml              # Horizontal Pod Autoscaler
    ├── externalsecret.yaml   # External Secrets sync config
    ├── secretstore.yaml      # AWS Secrets Manager connection
    └── serviceaccount.yaml   # ServiceAccount (IRSA)
```

## 🚀 Deployment

### Prerequisites

1. **EKS Cluster**: `frm` cluster must exist (managed by `FarmRegistry.Infrastructure` repository)
2. **Namespace**: `frm` namespace created
3. **Add-ons**: AWS Load Balancer Controller and External Secrets Operator installed
4. **ECR Image**: Docker image pushed to ECR
5. **AWS Secrets**: Connection string and JWT config in AWS Secrets Manager

### Deploy with Helm

```bash
# Update kubeconfig
aws eks update-kubeconfig --name frm --region us-east-1

# Deploy
helm upgrade --install frm-user-api ./k8s \
  --namespace frm \
  --create-namespace \
  --set image.tag=latest \
  --wait

# Check status
kubectl get pods -n fcg -l app.kubernetes.io/name=frm-user-api
kubectl get ingress -n fcg
```

### Override Values

```bash
# Use custom values file
helm upgrade --install frm-user-api ./k8s \
  --namespace frm \
  -f custom-values.yaml

# Override specific values
helm upgrade --install frm-user-api ./k8s \
  --namespace frm \
  --set image.tag=v1.2.3 \
  --set replicaCount=3 \
  --set autoscaling.maxReplicas=10
```

## ⚙️ Configuration

### Key Values

| Value | Default | Description |
|-------|---------|-------------|
| `image.repository` | `478511033947.dkr.ecr.us-east-1.amazonaws.com/frm-user-api` | ECR repository |
| `image.tag` | `latest` | Image tag (overridden by CI/CD with git SHA) |
| `replicaCount` | `2` | Initial number of pods |
| `containerPort` | `5067` | ASP.NET Core listening port |
| `service.type` | `ClusterIP` | Kubernetes service type |
| `service.port` | `80` | Service port |
| `ingress.enabled` | `true` | Enable ALB ingress |
| `ingress.className` | `alb` | Ingress controller class |
| `autoscaling.enabled` | `true` | Enable HPA |
| `autoscaling.minReplicas` | `2` | Minimum pods |
| `autoscaling.maxReplicas` | `5` | Maximum pods |
| `serviceAccount.create` | `false` | ServiceAccount created by eksctl (IRSA) |
| `serviceAccount.name` | `frm-user-api-sa` | ServiceAccount name |

### Environment Variables

Non-sensitive environment variables are configured in `values.yaml`:

```yaml
env:
  ASPNETCORE_URLS: "http://+:5067"
  OTEL_EXPORTER_OTLP_ENDPOINT: "http://otel-collector:4317"
```

### Secrets

Secrets are automatically synced from AWS Secrets Manager via External Secrets Operator:

- **Connection String**: `frm-api-user-connection-string`
- **JWT Config**: `frm-jwt-config`

The `ExternalSecret` resource watches these secrets and creates a Kubernetes Secret named `frm-user-api-secrets`.

## 🔒 Security

### IRSA (IAM Roles for Service Accounts)

The deployment uses IRSA to grant pods AWS permissions without storing credentials:

1. **ServiceAccount**: `frm-user-api-sa` (in `frm` namespace)
2. **IAM Role**: Auto-created by eksctl with OIDC trust
3. **IAM Policy**: `FRMExternalSecretsPolicy` (allows reading secrets)

**Created by CI/CD workflow:**
```bash
eksctl create iamserviceaccount \
  --cluster=frm \
  --namespace=frm \
  --name=frm-user-api-sa \
  --attach-policy-arn=arn:aws:iam::478511033947:policy/FRMExternalSecretsPolicy \
  --approve \
  --override-existing-serviceaccounts
```

### Resource Limits

```yaml
resources:
  requests:
    cpu: 100m      # Minimum CPU
    memory: 128Mi  # Minimum memory
  limits:
    cpu: 500m      # Maximum CPU
    memory: 512Mi  # Maximum memory
```

## 🌐 Networking

### Service

- **Type**: ClusterIP (internal only)
- **Port**: 80
- **Target Port**: 5067 (container port)

### Ingress (ALB)

- **Class**: `alb`
- **Scheme**: `internet-facing`
- **Target Type**: `ip`
- **Group**: `frm` (shared with other APIs)
- **Health Check**: `/health`
- **Protocol**: HTTP (port 80)

**Annotations:**
```yaml
alb.ingress.kubernetes.io/scheme: internet-facing
alb.ingress.kubernetes.io/target-type: ip
alb.ingress.kubernetes.io/group.name: fcg  # Shares ALB with other services
alb.ingress.kubernetes.io/healthcheck-path: /health
```

### Path-Based Routing

Multiple APIs can share the same ALB by using the `group.name` annotation. Each API defines its own path:

```yaml
# User API
path: /

# Future: Order API could use
path: /orders
```

## 📊 Autoscaling

### Horizontal Pod Autoscaler (HPA)

Automatically scales pods based on CPU and memory utilization:

```yaml
autoscaling:
  enabled: true
  minReplicas: 2    # Minimum pods (high availability)
  maxReplicas: 5    # Maximum pods (cost control)
  cpu: 60          # Scale up at 60% CPU
  memory: 70       # Scale up at 70% memory
```

**Scaling behavior:**
- Starts with 2 pods
- Scales up when CPU > 60% OR memory > 70%
- Scales down when below thresholds (with cooldown)
- Never goes below 2 or above 5 pods

## 🔍 Monitoring

### Health Checks

```bash
# Get ALB URL
ALB_URL=$(kubectl get ingress -n frm frm-user-api-frm-user-api -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')

# Test health endpoint
curl http://$ALB_URL/health
```

### Pod Status

```bash
# List pods
kubectl get pods -n frm -l app.kubernetes.io/name=frm-user-api

# Watch pod status
kubectl get pods -n frm -l app.kubernetes.io/name=frm-user-api -w

# Describe pod
kubectl describe pod -n frm <pod-name>

# View logs
kubectl logs -n frm <pod-name>

# Follow logs
kubectl logs -n frm <pod-name> -f
```

### Resource Usage

```bash
# Pod metrics (requires metrics-server)
kubectl top pods -n frm -l app.kubernetes.io/name=frm-user-api

# HPA status
kubectl get hpa -n frm frm-user-api
kubectl describe hpa -n frm frm-user-api
```

### External Secrets Status

```bash
# Check ExternalSecret
kubectl get externalsecret -n frm frm-user-api-secrets
kubectl describe externalsecret -n frm frm-user-api-secrets

# Check if Kubernetes Secret was created
kubectl get secret -n frm frm-user-api-secrets
# Verify secret data (base64 encoded)
kubectl get secret -n frm frm-user-api-secrets -o yaml
```

## 🧹 Cleanup

### Uninstall Helm Release

```bash
helm uninstall frm-user-api -n frm
```

This removes:
- Deployment
- Service
- Ingress (and associated ALB)
- HPA
- ExternalSecret
- SecretStore

**Note**: Does NOT remove:
- ServiceAccount (managed by eksctl)
- ECR images
- AWS Secrets Manager secrets
- EKS cluster

### Remove ServiceAccount (IRSA)

```bash
eksctl delete iamserviceaccount \
  --cluster=frm \
  --namespace=frm \
  --name=frm-user-api-sa \
  --region=us-east-1
```

## 🔧 Troubleshooting

### Helm Installation Failed

```bash
# Get Helm release status
helm status frm-user-api -n frm

# View Helm history
helm history frm-user-api -n frm

# Rollback to previous version
helm rollback frm-user-api -n frm
```

### Template Rendering Issues

```bash
# Dry-run to see rendered templates
helm install frm-user-api ./k8s \
  --namespace frm \
  --dry-run \
  --debug

# Render templates without installing
helm template frm-user-api ./k8s \
  --namespace frm \
  --set image.tag=test
```

### Update Values After Deployment

```bash
# Modify values.yaml, then upgrade
helm upgrade frm-user-api ./k8s \
  --namespace frm \
  --wait

# Force recreation of pods
helm upgrade frm-user-api ./k8s \
  --namespace frm \
  --recreate-pods \
  --wait
```

## 📚 References

- [Helm Documentation](https://helm.sh/docs/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [AWS Load Balancer Controller](https://kubernetes-sigs.github.io/aws-load-balancer-controller/)
- [External Secrets Operator](https://external-secrets.io/)
- [Infrastructure Documentation](../docs/INFRASTRUCTURE.md)
