#!/bin/bash
set -e

NAMESPACE="eshop-microservices"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CERTS_DIR="${SCRIPT_DIR}/../base/00-core/certs"

if [ ! -d "$CERTS_DIR" ]; then
  echo "ERROR: $CERTS_DIR not found."
  echo "Run: mkdir -p k8s/base/00-core/certs && cd src/certificates && ./generate-certs.sh"
  exit 1
fi

check_cert() {
  local file="$CERTS_DIR/$1"
  if [ ! -f "$file" ]; then
    echo "ERROR: Missing cert file: $file"
    echo "Run: cd src/certificates && ./generate-certs.sh"
    exit 1
  fi
}

check_cert "catalog.pfx"
check_cert "basket.pfx"
check_cert "discount.pfx"
check_cert "identity.pfx"
check_cert "order-command-api.pfx"
check_cert "order-query-event-api.pfx"
check_cert "order-query-event-processor.pfx"
check_cert "api-gateway.pfx"
check_cert "ca.crt"
check_cert "eshop-local.crt"
check_cert "eshop-local.key"

echo "=== Creating cert Secrets in namespace: $NAMESPACE ==="

create_cert_secret() {
  local SECRET_NAME="$1"
  local PFX_KEY="$2"
  local PFX_FILE="$CERTS_DIR/$3"

  kubectl create secret generic "$SECRET_NAME" \
    --from-file="${PFX_KEY}=${PFX_FILE}" \
    --namespace="$NAMESPACE" \
    --dry-run=client -o yaml | kubectl apply -f -
  echo "  ✓ $SECRET_NAME"
}

create_cert_secret "cert-catalog-api"                "catalog.pfx"                     "catalog.pfx"
create_cert_secret "cert-basket-api"                 "basket.pfx"                      "basket.pfx"
create_cert_secret "cert-discount-grpc"              "discount.pfx"                    "discount.pfx"
create_cert_secret "cert-identity-api"               "identity.pfx"                    "identity.pfx"
create_cert_secret "cert-order-command-api"          "order-command-api.pfx"           "order-command-api.pfx"
create_cert_secret "cert-order-query-api"            "order-query-event-api.pfx"       "order-query-event-api.pfx"
create_cert_secret "cert-order-query-eventprocessor" "order-query-event-processor.pfx" "order-query-event-processor.pfx"
create_cert_secret "cert-api-gateway"                "api-gateway.pfx"                 "api-gateway.pfx"

echo "=== Creating ingress TLS secret ==="
kubectl create secret tls ingress-tls \
  --cert="${CERTS_DIR}/eshop-local.crt" \
  --key="${CERTS_DIR}/eshop-local.key" \
  --namespace="$NAMESPACE" \
  --dry-run=client -o yaml | kubectl apply -f -
echo "  ✓ ingress-tls"

echo "=== Creating CA ConfigMap ==="
kubectl create configmap cert-ca \
  --from-file="ca.crt=${CERTS_DIR}/ca.crt" \
  --namespace="$NAMESPACE" \
  --dry-run=client -o yaml | kubectl apply -f -
echo "  ✓ cert-ca (ConfigMap)"

echo ""
echo "=== All cert secrets created successfully ==="
