#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
K8S_DIR="${SCRIPT_DIR}/.."
NAMESPACE="eshop-microservices"
OVERLAY="${1:-local}"

echo "=== EshopMicroservices K8s Deployment ==="
echo "Overlay: $OVERLAY"
echo ""

if ! kubectl cluster-info &>/dev/null; then
  echo "ERROR: kubectl is not connected to a cluster."
  echo "For minikube: minikube start"
  echo "For Docker Desktop: enable Kubernetes in Docker Desktop settings"
  exit 1
fi

echo "=== Ensuring namespace exists ==="
kubectl apply -f "${K8S_DIR}/base/00-core/namespace.yaml"
echo ""

if ! kubectl get secret cert-catalog-api -n "$NAMESPACE" &>/dev/null; then
  echo "Cert secrets not found. Running create-cert-secrets.sh..."
  echo ""
  bash "${SCRIPT_DIR}/create-cert-secrets.sh"
  echo ""
fi

echo "=== Applying overlay: $OVERLAY ==="
kubectl apply -k "${K8S_DIR}/overlays/${OVERLAY}"

echo ""
echo "=== Deployment complete ==="
echo "Watch rollout: kubectl get pods -n $NAMESPACE -w"
echo "Check status:  kubectl get all -n $NAMESPACE"
