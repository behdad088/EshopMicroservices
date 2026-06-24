#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
K8S_DIR="${SCRIPT_DIR}/.."
NAMESPACE="eshop-microservices"
OVERLAY="${1:-local}"

echo "=== EshopMicroservices K8s Teardown ==="
echo "Overlay: $OVERLAY"
echo ""

if ! kubectl cluster-info &>/dev/null; then
  echo "ERROR: kubectl is not connected to a cluster."
  exit 1
fi

kubectl delete -k "${K8S_DIR}/overlays/${OVERLAY}" --ignore-not-found

if [[ "${2}" == "--wipe-data" ]]; then
  echo ""
  echo "=== Deleting PVCs (--wipe-data flag set) ==="
  kubectl delete pvc --all -n "$NAMESPACE" --ignore-not-found
  echo "All PVCs deleted."
fi

echo ""
echo "=== Teardown complete ==="
echo "To redeploy: bash k8s/scripts/apply.sh $OVERLAY"
