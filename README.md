# FarmRegistry

Plataforma de Gestão de Propriedades Rurais (MVP) desenvolvida em .NET 8.0, criada como parte do Hackathon 8NETT, com foco no registro e organização de propriedades e talhões agrícolas.

## 🎯 Objetivo do Projeto

O FarmRegistry tem como objetivo fornecer uma API REST responsável pelo cadastro e gerenciamento estrutural do ambiente agrícola, servindo como base para integrações futuras com outros microserviços da plataforma.

Este serviço atuará como o registro central (registry) de propriedades e talhões.

## 🏗️ Arquitetura

A solução segue um modelo de arquitetura em camadas (DDD Light), visando clareza, separação de responsabilidades e facilidade de evolução.

### Estrutura da Solução

```
src/
 +- FarmRegistry.Api
 +- FarmRegistry.Application
 +- FarmRegistry.Domain
 +- FarmRegistry.Infrastructure

tests/
 +- FarmRegistry.Api.Tests
 +- FarmRegistry.Application.Tests
 +- FarmRegistry.Domain.Tests
 +- FarmRegistry.Infrastructure.Tests
```

## 📦 Responsabilidade das Camadas

**FarmRegistry.Api**  
Exposição da API REST (controllers, endpoints, Swagger).

**FarmRegistry.Application**  
Casos de uso, DTOs, validações e orquestração do fluxo da aplicação.

**FarmRegistry.Domain**  
Entidades e regras centrais do domínio de propriedades rurais.

**FarmRegistry.Infrastructure**  
Persistência, acesso a dados e integrações técnicas.

**Projetos de Testes**  
Preparados para testes unitários e de integração de cada camada.

## 📋 Escopo do MVP (CRUD)

De acordo com os requisitos do Hackathon 8NETT, o MVP deste microserviço contempla o CRUD das seguintes entidades:

### 🚜 Propriedade Rural
- Cadastro de propriedades
- Consulta de propriedades cadastradas
- Atualização de dados da propriedade
- Exclusão de propriedades

### 🌾 Talhão
- Cadastro de talhões vinculados a uma propriedade
- Consulta de talhões por propriedade
- Atualização de dados do talhão
- Exclusão de talhões

*Este serviço não é responsável por informações operacionais, leituras de sensores ou dados produtivos.*

## 🚀 Execução Local com Docker

### Pré-requisitos

- [Docker](https://www.docker.com/get-started/) instalado

### Comandos para rodar

Na raiz do projeto, execute: