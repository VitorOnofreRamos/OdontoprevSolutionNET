@echo ff

cd docker
docker-compose down
docker-compose up --build -d
docker-compose logs -f