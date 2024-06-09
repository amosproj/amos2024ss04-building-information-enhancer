group "default" {
  targets = ["frontend", "api-gateway", "datapipeline", "sql-database"]
}

target "frontend" {
  context = "./frontend"
  dockerfile = "./frontend/Dockerfile"
  tags = [
    "ghcr.io/amosproj/amos2024ss04-building-information-enhancer-frontend:${{STAGE}}"
  ]
  labels = {
    "stage" = "${{STAGE}}"
  }
  args = {
    "STAGE" = "${{STAGE}}"
  }
}

target "api-gateway" {
  context = "./backend/src/BIE.Core"
  dockerfile = "./backend/src/BIE.Core/Dockerfile"
  tags = [
    "ghcr.io/amosproj/amos2024ss04-building-information-enhancer-api-gateway:${{STAGE}}"
  ]
  labels = {
    "stage" = "${{STAGE}}"
  }
  args = {
    "STAGE" = "${{STAGE}}"
  }
}

target "datapipeline" {
  context = "./backend/src/BIE.DataPipeline"
  dockerfile = "./backend/src/BIE.DataPipeline/Dockerfile"
  tags = [
    "ghcr.io/amosproj/amos2024ss04-building-information-enhancer-datapipeline:${{STAGE}}"
  ]
  labels = {
    "stage" = "${{STAGE}}"
  }
  args = {
    "STAGE" = "${{STAGE}}"
  }
}

target "sql-database" {
  context = "./backend/database"
  dockerfile = "./backend/database/Dockerfile"
  tags = [
    "ghcr.io/amosproj/amos2024ss04-building-information-enhancer-sql-database:${{STAGE}}"
  ]
  labels = {
    "stage" = "${{STAGE}}"
  }
  args = {
    "STAGE" = "${{STAGE}}"
  }
}
