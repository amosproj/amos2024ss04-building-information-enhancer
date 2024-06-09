group "default" {
  targets = [
    "frontend",
    "api-gateway",
    "api-composer",
    "datapipeline",
    "sql-database",
    "metadata-database"
  ]
}

target "docker-metadata-action" {
  labels = {
    "org.opencontainers.image.created" = "${{ steps.meta.outputs.created }}"
    "org.opencontainers.image.version" = "${{ steps.meta.outputs.version }}"
    "org.opencontainers.image.revision" = "${{ steps.meta.outputs.revision }}"
    "org.opencontainers.image.source" = "${{ steps.meta.outputs.source }}"
  }
}

target "frontend" {
  inherits = ["docker-metadata-action"]
  context = "./frontend"
  dockerfile = "Dockerfile"
  tags = ["${REGISTRY}/${IMAGE_NAME}-frontend:${STAGE}"]
}

target "api-gateway" {
  inherits = ["docker-metadata-action"]
  context = "./backend/api-gateway"
  dockerfile = "Dockerfile"
  tags = ["${REGISTRY}/${IMAGE_NAME}-api-gateway:${STAGE}"]
}

target "api-composer" {
  inherits = ["docker-metadata-action"]
  context = "./backend/src/BIE.Core"
  dockerfile = "Dockerfile"
  tags = ["${REGISTRY}/${IMAGE_NAME}-api-composer:${STAGE}"]
}

target "datapipeline" {
  inherits = ["docker-metadata-action"]
  context = "./backend/src/BIE.DataPipeline"
  dockerfile = "Dockerfile"
  tags = ["${REGISTRY}/${IMAGE_NAME}-datapipeline:${STAGE}"]
}

target "sql-database" {
  inherits = ["docker-metadata-action"]
  context = "./backend/sql-database"
  dockerfile = "Dockerfile"
  tags = ["${REGISTRY}/${IMAGE_NAME}-sql-database:${STAGE}"]
}

target "metadata-database" {
  inherits = ["docker-metadata-action"]
  context = "./backend/metadata-database"
  dockerfile = "Dockerfile"
  tags = ["${REGISTRY}/${IMAGE_NAME}-metadata-database:${STAGE}"]
}
