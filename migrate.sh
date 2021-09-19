#!/usr/bin/env bash

context="Default"
name=
clear=false
update=false
rollback=false
output=

for i in "$@"; do
    case $1 in
        -c|--context) context="$2"; shift ;;
        -n|--name) name="$2"; shift ;;
        -c|--clear) clear=true ;;
        -u|--update) update=true ;;
        -r|--rollback) rollback=true ;;
        -o|--output) output="$2"; break ;;
    esac
    shift
done

context_name="${context}Context"

if [[ "$clear" == true ]]; then
    dotnet ef database drop -f --context ${context_name} --project src/Persistence --startup-project src/Web
    if [[ -d "src/Persistence/Migrations/${context}" ]]; then
        rm -r -f "src/Persistence/Migrations/${context}"
    fi
else 
    if [[ "$rollback" == true ]]; then
        dotnet ef migrations remove --force --context ${context_name}  --project src/Persistence --startup-project src/Web
    fi
fi

if [[ "$output" != "" ]]; then
    now=$(date '+%Y%m%d')
    script_name="${now}_${context}Context_${output}"
    dotnet ef migrations script -i --context ${context_name} -o src/Persistence/Migrations/${context}/Scripts/$script_name.sql  --project src/Persistence --startup-project src/Web
    echo "Done: ${script_name}"
else
    if [[ "$name" != "" ]]; then
        dotnet ef migrations add ${name} -o "Migrations/${context}" --context ${context_name}  --project src/Persistence --startup-project src/Web
        if [[ "$update" == true ]]; then
            dotnet ef database update ${name} --context ${context_name} --project src/Persistence --startup-project src/Web
        fi
    else
        if [[ "$update" == true ]]; then
            dotnet ef database update --context ${context_name} --project src/Persistence --startup-project src/Web
        fi
    fi
fi