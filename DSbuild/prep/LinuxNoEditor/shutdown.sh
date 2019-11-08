#!/bin/sh
BASIC_CREDS="<INSERT-YOUR-BASIC-CREDS-HERE>"
namespace="<INSERT-YOUR-NAMESPACE-HERE>"
DSM_SERVICE_URL="http://justice-dsm-service/dsm"
IAM_SERVICE_URL="http://justice-iam-service/iam"

count=0

while [ true ]; do
    response=$(curl "localhost:$PORT/match/status")
    match_id=$(echo "$response" | grep match_id | cut -d ':' -f 2 | cut -d '"' -f 2)
    match_state=$(echo "$response" | grep match_state | cut -d ':' -f 2 | cut -d '"' -f 2)
    players=$(echo "$response" | grep players | cut -d ':' -f 2 | cut -d ' ' -f 2)
    echo "match id: $match_id"
    echo "match state: $match_state"
    echo "players: $players"
    if [ "$match_id" != "" ]; then
        if [ "$match_state" = "InProgress" ]; then
            echo "Match has been started..!"
            if [ "$players" = "[]" ];then
                if [ "$count" -lt "10" ]; then
                    echo $count
                    count=$(( $count + 1 ))
                else
                    break
                fi
            fi
        fi
    fi
    sleep 3
done

echo "There is no one in the match, DS will terminated..!"

response=$(curl -X POST --header "Content-Type: application/x-www-form-urlencoded" --header "Accept: application/json" --header "Authorization: Basic $BASIC_CREDS" -d "grant_type=client_credentials" "$IAM_SERVICE_URL/oauth/token")
token=$(echo "$response" | grep access_token | cut -d ':' -f 2 | cut -d '"' -f 2)
curl -X POST --header "Content-Type: application/json" --header "Authorization: Bearer $token" -d "{ \"kill_me\": true, \"pod_name\": \"$POD_NAME\", \"session_id\": \"$match_id\" }" "$DSM_SERVICE_URL/namespaces/$namespace/servers/shutdown"
