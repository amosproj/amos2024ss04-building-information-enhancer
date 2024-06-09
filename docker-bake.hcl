target "docker-metadata-action" {}

target "frontend" {
  inherits = ["docker-metadata-action"]
  context = "./frontend"
  dockerfile = "./frontend/Dockerfile"
}

target "api-gateway" {
  inherits = ["docker-metadata-action"]
  context = "./backend/src/BIE.Core"
  dockerfile = "./backend/src/BIE.Core/Dockerfile"
}

target "datapipeline" {
  inherits = ["docker-metadata-action"]
  context = "./backend/src/BIE.DataPipeline"
  dockerfile = "./backend/src/BIE.DataPipeline/Dockerfile"
}

target "sql-database" {
  inherits = ["docker-metadata-action"]
  context = "./backend/database"
  dockerfile = "./backend/database/Dockerfile"
}
