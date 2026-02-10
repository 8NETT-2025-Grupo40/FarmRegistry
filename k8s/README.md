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

1. **EKS Cluster**: `agro-tech` cluster must exist
2. **Namespace**: `farm-registry` namespace created
3. **Add-ons**: AWS Load Balancer Controller and External Secrets Operator installed
4. **ECR Image**: Docker image pushed to ECR
5. **AWS Secrets**: `farm-registry-connection-string` and `farm-registry-cognito-config` in AWS Secrets Manager
6. **IAM Policy**: `infrastructure/iam/farm-registry-policy.json` created in IAM and attached to the IRSA ServiceAccount

### Deploy with Helm

```bash
# Update kubeconfig
aws eks update-kubeconfig --name agro-tech --region us-east-1

# Deploy
helm upgrade --install farm-registry ./k8s \
  --namespace farm-registry \
  --create-namespace \
  --set image.tag=latest \
  --wait

# Check status
kubectl get pods -n farm-registry -l app.kubernetes.io/name=farm-registry-api
kubectl get ingress -n farm-registry
```

### Override Values

```bash
# Use custom values file
helm upgrade --install farm-registry ./k8s \
  --namespace farm-registry \
  -f custom-values.yaml

# Override specific values
helm upgrade --install farm-registry ./k8s \
  --namespace farm-registry \
  --set image.tag=v1.2.3 \
  --set replicaCount=3 \
  --set autoscaling.maxReplicas=10
```

## ⚙️ Configuration

### Key Values

| Value | Default | Description |
|-------|---------|-------------|
| `image.repository` | `431104159307.dkr.ecr.us-east-1.amazonaws.com/farm-registry-api` | ECR repository |
| `image.tag` | `latest` | Image tag (overridden by CI/CD with git SHA) |
| `replicaCount` | `1` | Initial number of pods |
| `containerPort` | `5067` | ASP.NET Core listening port |
| `service.type` | `ClusterIP` | Kubernetes service type |
| `service.port` | `80` | Service port |
| `ingress.enabled` | `true` | Enable ALB ingress |
| `ingress.className` | `alb` | Ingress controller class |
| `autoscaling.enabled` | `true` | Enable HPA |
| `autoscaling.minReplicas` | `1` | Minimum pods |
| `autoscaling.maxReplicas` | `3` | Maximum pods |
| `serviceAccount.create` | `false` | ServiceAccount created by eksctl (IRSA) |
| `serviceAccount.name` | `farm-registry-api-sa` | ServiceAccount name |

### Environment Variables

Non-sensitive environment variables are configured in `values.yaml`:

```yaml
env:
  ASPNETCORE_URLS: "http://+:5067"
  OTEL_EXPORTER_OTLP_ENDPOINT: "http://cloudwatch-agent.amazon-cloudwatch.svc.cluster.local:4315"
  Authentication__AuthMode: "COGNITO"
```

### Secrets

Secrets are automatically synced from AWS Secrets Manager via External Secrets Operator:

- **Connection String**: `farm-registry-connection-string`
- **Cognito Config**: `farm-registry-cognito-config`

**Cognito config JSON format:**
```json
{
  "region": "us-east-1",
  "userPoolId": "us-east-1_1VGl4idF2",
  "clientId": "11sggm00trda1etpmimiqah1cp"
}
```

The `ExternalSecret` resource watches these secrets and creates a Kubernetes Secret named `farm-registry-farm-registry-api-secrets`.

## 🔒 Security

### IRSA (IAM Roles for Service Accounts)

The deployment uses IRSA to grant pods AWS permissions without storing credentials:

1. **ServiceAccount**: `farm-registry-api-sa` (in `farm-registry` namespace)
2. **IAM Role**: Auto-created by eksctl with OIDC trust
3. **IAM Policy**: `FarmRegistryAPIPolicy` (allows reading secrets)

**Created by CI/CD workflow:**
```bash
eksctl create iamserviceaccount \
  --cluster=agro-tech \
  --namespace=farm-registry \
  --name=farm-registry-api-sa \
  --attach-policy-arn=arn:aws:iam::431104159307:policy/FarmRegistryAPIPolicy \
  --approve \
  --override-existing-serviceaccounts
```

### Resource Limits

```yaml
resources:
  requests:
    cpu: 150m      # Minimum CPU
    memory: 256Mi  # Minimum memory
  limits:
    cpu: 750m      # Maximum CPU
    memory: 768Mi  # Maximum memory
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
- **Group**: `agro-tech` (shared with other APIs)
- **Health Check**: `/registry/ready`
- **Protocol**: HTTP (port 80)

**Annotations:**
```yaml
alb.ingress.kubernetes.io/scheme: internet-facing
alb.ingress.kubernetes.io/target-type: ip
alb.ingress.kubernetes.io/group.name: agro-tech  # Shares ALB with other services
alb.ingress.kubernetes.io/healthcheck-path: /registry/ready
```

### Path-Based Routing

Multiple APIs can share the same ALB by using the `group.name` annotation. Each API defines its own path:

```yaml
# Farm Registry API
path: /registry

# Future: Order API could use
path: /orders
```

## 📊 Autoscaling

### Horizontal Pod Autoscaler (HPA)

Automatically scales pods based on CPU and memory utilization:

```yaml
autoscaling:
  enabled: true
  minReplicas: 1    # Minimum pods
  maxReplicas: 3    # Maximum pods
  cpu: 60          # Scale up at 60% CPU
  memory: 70       # Scale up at 70% memory
```

**Scaling behavior:**
- Starts with 1 pod
- Scales up when CPU > 60% OR memory > 70%
- Scales down when below thresholds (with cooldown)
- Never goes below 1 or above 3 pods

## 🔍 Monitoring

### Health Checks

```bash
# Get ALB URL
ALB_URL=$(kubectl get ingress -n farm-registry farm-registry-farm-registry-api -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')

# Test readiness endpoint
curl http://$ALB_URL/registry/ready
```

### Pod Status

```bash
# List pods
kubectl get pods -n farm-registry -l app.kubernetes.io/name=farm-registry-api

# Watch pod status
kubectl get pods -n farm-registry -l app.kubernetes.io/name=farm-registry-api -w

# Describe pod
kubectl describe pod -n farm-registry <pod-name>

# View logs
kubectl logs -n farm-registry <pod-name>

# Follow logs
kubectl logs -n farm-registry <pod-name> -f
```

### Resource Usage

```bash
# Pod metrics (requires metrics-server)
kubectl top pods -n farm-registry -l app.kubernetes.io/name=farm-registry-api

# HPA status
kubectl get hpa -n farm-registry farm-registry-farm-registry-api
kubectl describe hpa -n farm-registry farm-registry-farm-registry-api
```

### External Secrets Status

```bash
# Check ExternalSecret
kubectl get externalsecret -n farm-registry farm-registry-farm-registry-api-externalsecret
kubectl describe externalsecret -n farm-registry farm-registry-farm-registry-api-externalsecret

# Check if Kubernetes Secret was created
kubectl get secret -n farm-registry farm-registry-farm-registry-api-secrets
# Verify secret data (base64 encoded)
kubectl get secret -n farm-registry farm-registry-farm-registry-api-secrets -o yaml
```

## 🧹 Cleanup

### Uninstall Helm Release

```bash
helm uninstall farm-registry -n farm-registry
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
  --cluster=agro-tech \
  --namespace=farm-registry \
  --name=farm-registry-api-sa \
  --region=us-east-1
```

## 🔧 Troubleshooting

### Helm Installation Failed

```bash
# Get Helm release status
helm status farm-registry -n farm-registry

# View Helm history
helm history farm-registry -n farm-registry

# Rollback to previous version
helm rollback farm-registry -n farm-registry
```

### Template Rendering Issues

```bash
# Dry-run to see rendered templates
helm install farm-registry ./k8s \
  --namespace farm-registry \
  --dry-run \
  --debug

# Render templates without installing
helm template farm-registry ./k8s \
  --namespace farm-registry \
  --set image.tag=test
```

### Update Values After Deployment

```bash
# Modify values.yaml, then upgrade
helm upgrade farm-registry ./k8s \
  --namespace farm-registry \
  --wait

# Force recreation of pods
helm upgrade farm-registry ./k8s \
  --namespace farm-registry \
  --recreate-pods \
  --wait
```

## 📚 References

- [Helm Documentation](https://helm.sh/docs/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [AWS Load Balancer Controller](https://kubernetes-sigs.github.io/aws-load-balancer-controller/)
- [External Secrets Operator](https://external-secrets.io/)
- [Infrastructure Documentation](../docs/INFRASTRUCTURE.md)
