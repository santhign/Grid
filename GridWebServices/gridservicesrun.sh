#!/bin/bash
## declare an array of Services
spath="/home/ubuntu/grid"
declare -a arr=("CatelogService" "CustomerService" "AdminService" "OrderService" "NotificationService" )
declare -a arr1=("5124" "5125" "5126" "5127" "5128" )
declare interval=1m

if [ $# -gt 0 ]; then
  if [[ "$1" = "-stop" ]];  then
    read -p "Stopping services.. Are you sure? " -n 1 -r
    echo    # (optional) move to a new line
    if [[ $REPLY =~ ^[Yy]$ ]] ; then
      for p in "${arr1[@]}"
      do
	      sudo fuser -k $p/tcp
      done

    fi  
  elif [[ "$1" = "-start" ]]; then
   while true; do 
    echo "=================== START CHECKING SERVICES ==================="
    ## now loop through the above array
    for s in "${arr[@]}"
    do
      #echo -n 'Checking $s... : '
      ps -aux | grep "[d]otnet $s.dll" > /dev/null

      if [ $? -eq 0 ]; then
        echo "Service is running. No action taken. ($s)"
      else
        echo "$s is NOT RUNNING. Starting the Service."
        cd $spath/$s
        nohup dotnet $s.dll &
        cd ..
      fi
    done
    echo "========================== RUNNING in LOOP =========================="
    sleep $interval
   done
  else
    echo "invalid argument. please enter -start or -stop"
  fi
else
  echo "please enter -start or -stop as parameter"
fi
