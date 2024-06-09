group "default" {
  targets = ["frontend", "api-gateway", "datapipeline", "sql-database", "metadata-database"]
}

target "docker-metadata-action" {}

target "frontend" {
  inherits = ["docker-metadata-action"]
  context = "./frontend"
  dockerfile = "Dockerfile"
}

target "api-gateway" {
  inherits = ["docker-metadata-action"]
  context = "./backend/src/BIE.Core"
  dockerfile = "Dockerfile"
}

target "datapipeline" {
  inherits = ["docker-metadata-action"]
  context = "./backend/src/BIE.DataPipeline"
  dockerfile = "Dockerfile"
}

target "sql-database" {
  inherits = ["docker-metadata-action"]
  context = "./backend/sql-database"
  dockerfile = "Dockerfile"
}

target "metadata-database" {
  inherits = ["docker-metadata-action"]
  context = "./backend/metadata-database"
  dockerfile = "Dockerfile"
}
