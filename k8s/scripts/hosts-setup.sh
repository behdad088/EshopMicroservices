#!/bin/bash
# Prints /etc/hosts entries needed for *.eshop.local ingress rules.

detect_ip() {
  if command -v minikube &>/dev/null && minikube status &>/dev/null 2>&1; then
    minikube ip
    return
  fi

  # Docker Desktop maps the ingress LoadBalancer to localhost — its internal VM IP
  # (198.19.249.x) is not routable from Electron-based apps like Postman.
  local KUBE_CONTEXT
  KUBE_CONTEXT=$(kubectl config current-context 2>/dev/null)
  if [ "$KUBE_CONTEXT" = "docker-desktop" ]; then
    echo "127.0.0.1"
    return
  fi

  local LB_IP
  LB_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx \
    -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null)
  if [ -n "$LB_IP" ] && [ "$LB_IP" != "null" ]; then
    echo "$LB_IP"
    return
  fi

  echo "127.0.0.1"
}

INGRESS_IP=$(detect_ip)

echo "# EshopMicroservices — add these to /etc/hosts"
echo "# Detected ingress IP: $INGRESS_IP"
echo ""
echo "$INGRESS_IP  api.eshop.local"
echo "$INGRESS_IP  identity.eshop.local"
echo "$INGRESS_IP  mailpit.eshop.local"
echo "$INGRESS_IP  kibana.eshop.local"
echo "$INGRESS_IP  jaeger.eshop.local"
echo "$INGRESS_IP  grafana.eshop.local"
echo "$INGRESS_IP  prometheus.eshop.local"
echo "$INGRESS_IP  rabbitmq.eshop.local"
