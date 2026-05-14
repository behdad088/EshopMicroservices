#!/usr/bin/env bash
# Polls each service's /hc health check endpoint until it returns HTTP 200 or the
# per-service timeout is exceeded. Exits 1 if any service never becomes healthy.

set -euo pipefail

TIMEOUT_SECONDS=120
INTERVAL=5

services=(
    "identity.api|https://localhost:7063/hc"
    "catalog.api|http://localhost:6000/hc"
    "basket.api|http://localhost:6001/hc"
    "order.command.api|http://localhost:6003/hc"
    "order.query.eventprocessor|http://localhost:6004/hc"
    "order.query.api|http://localhost:6005/hc"
)

wait_for() {
    local name="$1"
    local url="$2"
    local deadline=$(( $(date +%s) + TIMEOUT_SECONDS ))

    echo "Waiting for ${name} at ${url} ..."
    while (( $(date +%s) < deadline )); do
        if curl -sfk --max-time 3 "${url}" > /dev/null 2>&1; then
        echo "  ✓ ${name} is healthy"
        return 0
    fi
        sleep "${INTERVAL}"
    done

    echo "  ✗ ${name} did not become healthy within ${TIMEOUT_SECONDS}s"
    return 1
}

failed=0
for entry in "${services[@]}"; do
    IFS='|' read -r name url <<< "${entry}"
    wait_for "${name}" "${url}" || failed=1
done

exit "${failed}"
