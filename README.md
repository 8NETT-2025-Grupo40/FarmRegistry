# FarmRegistry

Plataforma de Gestão de Propriedades Rurais (MVP) desenvolvida em .NET 8.0, criada como parte do Hackathon 8NETT, com foco no registro e organização de propriedades e talhões agrícolas.

O projeto está em fase inicial e, neste momento, contém apenas a estrutura da solução, sem implementação de regras ou funcionalidades.

## ?? Objetivo do Projeto

O FarmRegistry tem como objetivo fornecer uma API REST responsável pelo cadastro e gerenciamento estrutural do ambiente agrícola, servindo como base para integrações futuras com outros microserviços da plataforma.

Este serviço atuará como o registro central (registry) de propriedades e talhões.

## ?? Arquitetura

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

## ?? Responsabilidade das Camadas

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

## ?? Escopo do MVP (CRUD)

De acordo com os requisitos do Hackathon 8NETT, o MVP deste microserviço deverá contemplar o CRUD das seguintes entidades:

### ?? Propriedade Rural
- Cadastro de propriedades
- Consulta de propriedades cadastradas
- Atualização de dados da propriedade
- Exclusão de propriedades

### ?? Talhão
- Cadastro de talhões vinculados a uma propriedade
- Consulta de talhões por propriedade
- Atualização de dados do talhão
- Exclusão de talhões

*Este serviço não é responsável por informações operacionais, leituras de sensores ou dados produtivos.*

## ?? Autenticação (Direcionamento Futuro)

O projeto está sendo preparado para:
- Autenticação via AWS Cognito (JWT)
- Possibilidade de autenticação simulada (Mock) durante desenvolvimento local

A implementação da autenticação será realizada em etapas futuras.

## ?? Stack Tecnológica (Planejada)

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Docker
- AutoMapper
- FluentValidation
- Swagger / OpenAPI

## ?? Status Atual

- ? Estrutura da solução criada
- ? Projetos organizados por camada
- ? Projetos de testes criados
- ? Implementação do domínio (em andamento)
- ? Implementação dos casos de uso
- ? Implementação da API e persistência

## ?? Observações

Este README representa o estado inicial do projeto e tem como objetivo:

- Documentar a estrutura base da solução
- Definir claramente o escopo de responsabilidade do microserviço
- Servir como ponto de partida para evolução futura da plataforma

Detalhamentos técnicos mais profundos serão adicionados conforme o desenvolvimento avançar.