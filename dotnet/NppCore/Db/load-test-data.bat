@echo off
echo Loading test data into Cassandra...
docker exec -i cassandra cqlsh < test-podaci.sql
echo Done!
pause
