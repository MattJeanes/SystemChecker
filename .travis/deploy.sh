docker login -u $DOCKER_USER -p $DOCKER_PASS
docker push mattjeanes/systemchecker.service:$version
docker push mattjeanes/systemchecker.service
docker push mattjeanes/systemchecker.web:$version
docker push mattjeanes/systemchecker.web
docker push mattjeanes/systemchecker.migrations:$version
docker push mattjeanes/systemchecker.migrations