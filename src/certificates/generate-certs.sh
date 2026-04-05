#!/bin/bash
set -e

CERT_DIR="$(cd "$(dirname "$0")" && pwd)"
K8S_CERT_DIR="$(cd "$CERT_DIR/../k8s/00-core/certs" 2>/dev/null && pwd)"
CA_PASSWORD="dev"
CERT_PASSWORD="dev"
DAYS_VALID=365

echo "=== Generating Internal CA ==="
# Generate CA private key
openssl genrsa -out "$CERT_DIR/ca.key" 4096

# Generate CA certificate
openssl req -x509 -new -nodes \
  -key "$CERT_DIR/ca.key" \
  -sha256 \
  -days $DAYS_VALID \
  -out "$CERT_DIR/ca.crt" \
  -subj "/CN=EshopMicroservices Internal CA/O=EshopMicroservices"

echo "=== CA certificate created ==="

# Function to generate a service certificate signed by the CA
generate_service_cert() {
  local SERVICE_NAME=$1
  local DNS_NAMES=$2

  echo "--- Generating certificate for: $SERVICE_NAME ---"

  # Create a temporary config file for SAN (Subject Alternative Names)
  cat > "$CERT_DIR/${SERVICE_NAME}.cnf" <<EOF
[req]
default_bits = 2048
prompt = no
distinguished_name = dn
req_extensions = v3_req

[dn]
CN = ${SERVICE_NAME}

[v3_req]
subjectAltName = ${DNS_NAMES}
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
EOF

  # Generate private key
  openssl genrsa -out "$CERT_DIR/${SERVICE_NAME}.key" 2048

  # Generate CSR
  openssl req -new \
    -key "$CERT_DIR/${SERVICE_NAME}.key" \
    -out "$CERT_DIR/${SERVICE_NAME}.csr" \
    -config "$CERT_DIR/${SERVICE_NAME}.cnf"

  # Sign with CA
  openssl x509 -req \
    -in "$CERT_DIR/${SERVICE_NAME}.csr" \
    -CA "$CERT_DIR/ca.crt" \
    -CAkey "$CERT_DIR/ca.key" \
    -CAcreateserial \
    -out "$CERT_DIR/${SERVICE_NAME}.crt" \
    -days $DAYS_VALID \
    -sha256 \
    -extensions v3_req \
    -extfile "$CERT_DIR/${SERVICE_NAME}.cnf"

  # Export to PFX (needed by ASP.NET Core / Kestrel)
  openssl pkcs12 -export \
    -out "$CERT_DIR/${SERVICE_NAME}.pfx" \
    -inkey "$CERT_DIR/${SERVICE_NAME}.key" \
    -in "$CERT_DIR/${SERVICE_NAME}.crt" \
    -certfile "$CERT_DIR/ca.crt" \
    -passout pass:$CERT_PASSWORD

  # Clean up intermediate files
  rm -f "$CERT_DIR/${SERVICE_NAME}.csr" "$CERT_DIR/${SERVICE_NAME}.cnf"

  # Copy PFX to k8s certs directory
  if [ -d "$K8S_CERT_DIR" ]; then
    cp "$CERT_DIR/${SERVICE_NAME}.pfx" "$K8S_CERT_DIR/${SERVICE_NAME}.pfx"
    echo "    -> copied to k8s certs"
  fi

  echo "    -> ${SERVICE_NAME}.pfx created"
}

# Generate certificates for each service
# DNS names include Docker service names and Kubernetes service DNS names
NS="eshop-microservices"
generate_service_cert "catalog"                    "DNS:catalog.api,DNS:catalog-api,DNS:catalog-api.${NS},DNS:catalog-api.${NS}.svc.cluster.local,DNS:localhost"
generate_service_cert "basket"                     "DNS:basket.api,DNS:basket-api,DNS:basket-api.${NS},DNS:basket-api.${NS}.svc.cluster.local,DNS:localhost"
generate_service_cert "discount"                   "DNS:discount.grpc,DNS:discount-grpc,DNS:discount-grpc.${NS},DNS:discount-grpc.${NS}.svc.cluster.local,DNS:localhost"
generate_service_cert "identity"                   "DNS:identity.api,DNS:identity-api,DNS:identity-api.${NS},DNS:identity-api.${NS}.svc.cluster.local,DNS:localhost"
generate_service_cert "order-command-api"           "DNS:order.command.api,DNS:order-command-api,DNS:order-command-api.${NS},DNS:order-command-api.${NS}.svc.cluster.local,DNS:localhost"
generate_service_cert "order-query-event-api"       "DNS:order.query.api,DNS:order-query-api,DNS:order-query-api.${NS},DNS:order-query-api.${NS}.svc.cluster.local,DNS:localhost"
generate_service_cert "order-query-event-processor" "DNS:order.query.eventprocessor,DNS:order-query-eventprocessor,DNS:order-query-eventprocessor.${NS},DNS:order-query-eventprocessor.${NS}.svc.cluster.local,DNS:localhost"
generate_service_cert "api-gateway"                "DNS:api.gateway,DNS:api-gateway,DNS:api-gateway.${NS},DNS:api-gateway.${NS}.svc.cluster.local,DNS:localhost"

# Copy CA cert to k8s certs directory
if [ -d "$K8S_CERT_DIR" ]; then
  cp "$CERT_DIR/ca.crt" "$K8S_CERT_DIR/ca.crt"
  echo "=== CA certificate copied to k8s certs ==="
fi

echo ""
echo "=== All certificates generated ==="
echo "CA certificate: $CERT_DIR/ca.crt"
echo "CA private key: $CERT_DIR/ca.key (keep this secure!)"
echo ""
echo "All .pfx files use password: $CERT_PASSWORD"
