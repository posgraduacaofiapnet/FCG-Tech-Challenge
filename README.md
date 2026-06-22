# FCG (FIAP Cloud Games) - Fase 2 Microsservicos

Esta entrega adiciona uma arquitetura de microsservicos orientada a eventos para atender o Tech Challenge da Fase 2. O monolito original foi preservado em `src/` e a nova arquitetura foi adicionada em `services/`, pronta para ser separada em repositorios proprios.

## Arquitetura da Fase 2

- `services/UsersAPI`: cadastro, login, JWT Bearer e publicacao de `UserCreatedEvent`.
- `services/CatalogAPI`: catalogo de jogos, inicio de compra, biblioteca e consumo de `PaymentProcessedEvent`.
- `services/PaymentsAPI`: consumo de `OrderPlacedEvent`, simulacao de pagamento e publicacao de `PaymentProcessedEvent`.
- `services/NotificationsAPI`: consumo de eventos e simulacao de e-mails via logs.
- `contracts/FCG.Contracts`: contratos compartilhados dos eventos.
- `k8s/`: manifests de Kubernetes com Deployments, Services, ConfigMap e Secret.

Tecnologias mantidas/alinhadas ao monolito: .NET 10, Entity Framework Core 10, SQL Server, JWT Bearer, FluentValidation, Swagger/OpenAPI, xUnit, FluentAssertions e Docker Compose. A Fase 2 tambem usa RabbitMQ com MassTransit para mensageria.

## Executando com Docker Compose

Na raiz do repositorio:

```bash
docker compose up --build
```

Servicos expostos:

- UsersAPI: `http://localhost:5101/swagger`
- CatalogAPI: `http://localhost:5102/swagger`
- PaymentsAPI: `http://localhost:5103/swagger`
- NotificationsAPI: `http://localhost:5104/swagger`
- RabbitMQ Management: `http://localhost:15672` (`guest` / `guest`)
- SQL Server: `localhost,1433`

## Fluxo de Cadastro

1. Chame `POST http://localhost:5101/api/auth/register`.
2. O UsersAPI cria o usuario no SQL Server.
3. O UsersAPI publica `UserCreatedEvent`.
4. O NotificationsAPI consome o evento e registra no console o envio do e-mail de boas-vindas.

Exemplo:

```json
{
  "name": "User",
  "email": "user@email.com",
  "password": "Senha@123"
}
```

## Fluxo de Compra

1. Crie um jogo em `POST http://localhost:5102/api/games`.
2. Solicite a compra em `POST http://localhost:5102/api/library/purchase`.
3. O CatalogAPI publica `OrderPlacedEvent`.
4. O PaymentsAPI consome o pedido e publica `PaymentProcessedEvent` com status `Approved`.
5. O CatalogAPI consome o pagamento aprovado e adiciona o jogo a biblioteca.
6. O NotificationsAPI consome o pagamento aprovado e registra no console o e-mail de confirmacao.

Exemplo de compra:

```json
{
  "userId": "GUID_DO_USUARIO",
  "gameId": "GUID_DO_JOGO"
}
```

Consulte a biblioteca:

```http
GET http://localhost:5102/api/library/{userId}
```

## Kubernetes Local

Construa as imagens localmente com os nomes usados nos manifests:

```bash
docker build -t fcg-users-api:latest -f services/UsersAPI/Dockerfile .
docker build -t fcg-catalog-api:latest -f services/CatalogAPI/Dockerfile .
docker build -t fcg-payments-api:latest -f services/PaymentsAPI/Dockerfile .
docker build -t fcg-notifications-api:latest -f services/NotificationsAPI/Dockerfile .
```

Depois aplique os manifests:

```bash
kubectl apply -f k8s
kubectl get pods
kubectl get services
```

Os manifests usam:

- `Deployment` para todos os workloads.
- `Service` para comunicacao interna.
- `ConfigMap` para configuracoes nao sensiveis.
- `Secret` para connection strings, senha do SQL Server e chave JWT.

## Testes

```bash
dotnet test
```

Os testes antigos do monolito foram mantidos e os novos testes cobrem validacao de senha, publicacao de eventos, criacao de jogo, processamento de pagamento e notificacoes.

---

# FCG (FIAP Cloud Games) - Monolito Original

API REST para cadastro de usuarios, autenticacao, catalogo de jogos, fluxo de compra e biblioteca de jogos adquiridos.

O projeto usa Minimal APIs, Entity Framework Core com SQL Server, JWT Bearer, FluentValidation, Swagger e organizacao em camadas seguindo principios de Clean Architecture e DDD.

## Tecnologias

- .NET 10
- Entity Framework Core 10
- SQL Server
- JWT Bearer
- FluentValidation
- Swagger / OpenAPI
- xUnit e FluentAssertions
- Docker Compose

## Estrutura

- `src/FCG.API`: endpoints, middlewares, configuracao da API e composicao de dependencias.
- `src/FCG.Application`: services, DTOs, validadores, contratos e regras de aplicacao.
- `src/FCG.Domain`: entidades, enums, erros padronizados, Result Pattern e regras de dominio.
- `src/FCG.Infrastructure`: DbContext, migrations, repositories, Unit of Work e servicos de infraestrutura.
- `test/FCG.Test`: testes unitarios das principais regras.

## Regras Atendidas

### Cadastro de usuarios

O cadastro publico recebe nome, e-mail e senha.

A senha deve respeitar a politica minima:

- minimo de 8 caracteres;
- pelo menos uma letra;
- pelo menos um numero;
- pelo menos um caractere especial.

O e-mail e validado com FluentValidation.

### Autenticacao e autorizacao

A API usa JWT Bearer e possui dois perfis:

- `User`: acessa a plataforma, cria pedidos e consulta sua biblioteca.
- `Admin`: cadastra jogos, cria/desativa promocoes e administra usuarios.

Tokens incluem claims de id, nome, e-mail e role. Os logs estruturados incluem escopo com `UserId` e `UserEmail` para usuarios autenticados.

### Biblioteca de jogos adquiridos

O fluxo atual de aquisicao funciona por pedidos:

1. O usuario consulta o catalogo de jogos.
2. O usuario cria um pedido com um ou mais jogos.
3. O pagamento do pedido e aprovado.
4. Os jogos do pedido sao adicionados a biblioteca do usuario.

Usuarios nao podem consultar ou pagar pedidos de outros usuarios. Admins podem acessar pedidos para administracao.

### Administracao de usuarios

Endpoints protegidos por role `Admin` permitem:

- cadastrar usuarios com role `User` ou `Admin`;
- listar usuarios paginados, incluindo usuarios inativos;
- consultar usuario por id, incluindo usuarios inativos;
- alterar role do usuario.
- inativar usuarios por exclusao logica;
- reativar usuarios inativos.

O cadastro publico sempre cria usuarios com role `User`.

O usuario default `fgc_admin@admin.com` nao pode ser inativado pela API.
Usuarios inativos nao aparecem nas buscas comuns nem conseguem autenticar, mas continuam visiveis para Admin.

## Usuario Administrador Seedado

A aplicacao cria automaticamente um administrador no warmup, apos aplicar migrations.

```text
Email: fgc_admin@admin.com
Senha local padrao: Adm!n123
Role: Admin
```

A senha do admin e configuravel pela variavel de ambiente:

```text
Admin_Password
```

Essa senha tambem precisa seguir a politica forte: minimo de 8 caracteres, letras, numeros e caracteres especiais. Se uma senha fraca for configurada, a aplicacao falha no startup.

## Variaveis de Ambiente

```text
ConnectionStrings__DefaultConnection
chave_secreta
Admin_Password
Jwt_Key
Jwt_Issuer
Jwt_Audience
```

## Executando Todo o Projeto com Docker Compose

Este e o caminho recomendado para executar o projeto completo, subindo a API e o SQL Server juntos.

### 1. Pre-requisitos

Antes de iniciar, verifique se voce tem instalado:

- Docker Desktop ou Docker Engine;
- Docker Compose;
- uma porta local `1433` livre para o SQL Server;
- uma porta local `5169` livre para a API.

Para conferir se o Docker esta disponivel:

```bash
docker --version
docker compose version
```

Se seu ambiente usar o comando antigo, use `docker-compose` no lugar de `docker compose`.

### 2. Conferir variaveis usadas pelo compose

O arquivo `docker-compose.yml` ja configura tudo que a API precisa:

```text
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=FCGDb;User Id=sa;Password=ArquiteturaFiap.NET@2026;TrustServerCertificate=True;
chave_secreta=fgc_tech_challenge
Admin_Password=Adm!n123
Jwt_Key=zR8pW5vB9yX2mN4qA7L1jK9sT6uE3hG0fD5cX8vB2nN1mQ4wZ7xR0tY3uI6pP9oL
Jwt_Issuer=FCG.API
Jwt_Audience=FCG.API
```

A senha do SQL Server configurada no compose e:

```text
ArquiteturaFiap.NET@2026
```

A senha do admin default configurada no compose e:

```text
Adm!n123
```

Ela respeita a politica de senha forte da aplicacao.

### 3. Subir os containers

Na raiz do repositorio, execute:

```bash
docker compose up --build
```

Ou, usando o comando legado:

```bash
docker-compose up --build
```

Importante: suba pelo Compose na raiz do repositorio. Se voce executar a imagem manualmente pelo Docker Desktop, as variaveis do `docker-compose.yml` nao serao aplicadas e a API pode falhar com erro de configuracao obrigatoria, como `chave_secreta`.

Esse comando faz:

- build da imagem da API;
- download da imagem `mcr.microsoft.com/mssql/server:2022-latest`;
- criacao do container `fcg_sqlserver`;
- criacao do container `fcg_api`;
- aguarda o SQL Server ficar saudavel pelo healthcheck;
- inicia a API somente depois do banco estar pronto.

### 4. Aguardar a inicializacao

Na primeira execucao, o SQL Server pode levar alguns segundos para ficar pronto.

Quando a API iniciar, ela executa automaticamente:

- migrations do Entity Framework;
- seed do usuario admin default.

Mensagem esperada nos logs da API:

```text
Banco de dados atualizado com sucesso!
```

### 5. Acessar a API

```text
http://localhost:5169
```

Swagger:

```text
http://localhost:5169/swagger
```

### 6. Login com o admin default

No Swagger, chame:

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "fgc_admin@admin.com",
  "password": "Adm!n123"
}
```

Copie o token retornado e clique em `Authorize` no Swagger.

Informe:

```text
Bearer <token>
```

Com esse token voce consegue acessar os endpoints protegidos por role `Admin`.

### 7. Fluxo sugerido para testar

1. Fazer login como admin default.
2. Criar um jogo em `POST /api/games`.
3. Criar um usuario comum em `POST /api/auth/register`.
4. Fazer login com o usuario comum.
5. Criar um pedido em `POST /api/orders`.
6. Aprovar o pagamento em `POST /api/orders/{id}/pay`.
7. Consultar a biblioteca em `GET /api/library`.

### 7.1. Testando com Postman

O projeto possui uma collection pronta para testar os principais endpoints:

```text
postman/FCG.API.postman_collection.json
```

Para usar:

1. Abra o Postman.
2. Clique em `Import`.
3. Selecione o arquivo `postman/FCG.API.postman_collection.json`.
4. Execute primeiro o request `Autenticacao > 01 - Login Admin`.

Esse primeiro request usa o admin seedado:

```text
Email: fgc_admin@admin.com
Senha: Adm!n123
```

Ao executar o login, a collection salva automaticamente o token JWT na variavel `adminToken`.

Depois disso, siga os requests da collection na ordem sugerida para testar:

- administracao de usuarios;
- criacao de jogos;
- promocoes;
- registro/login de usuario comum;
- criacao de pedido;
- pagamento;
- consulta da biblioteca.

### 8. Rodar em segundo plano

Para deixar os containers em background:

```bash
docker compose up --build -d
```

Ver logs da API:

```bash
docker logs -f fcg_api
```

Ver logs do SQL Server:

```bash
docker logs -f fcg_sqlserver
```

Listar containers:

```bash
docker compose ps
```

Os nomes esperados dos containers sao:

```text
fcg_api
fcg_sqlserver
```

Se o Docker Desktop mostrar um container com nome aleatorio, por exemplo `infallible_margulis`, ele provavelmente foi iniciado manualmente pela imagem e nao pelo Compose. Remova esse container e suba novamente com `docker compose up --build`.

### 9. Parar os containers

```bash
docker compose down
```

Isso para e remove os containers da aplicacao, mas nao remove imagens.

### 10. Limpar tudo e recriar do zero

Se quiser apagar containers, rede e volumes anonimos criados pelo compose:

```bash
docker compose down -v
```

Depois suba novamente:

```bash
docker compose up --build
```

### 11. Problemas comuns

Se a porta `1433` ja estiver em uso, pare outro SQL Server local ou altere o mapeamento de porta no `docker-compose.yml`.

Se a porta `5169` ja estiver em uso, altere:

```yaml
ports:
  - "5169:8080"
```

para outra porta local, por exemplo:

```yaml
ports:
  - "5170:8080"
```

Se a API falhar no startup por senha fraca do admin, ajuste `Admin_Password` no `docker-compose.yml` para uma senha com minimo de 8 caracteres, letras, numeros e caracteres especiais.

## Executando Localmente

Suba o SQL Server localmente ou via Docker e execute:

```bash
dotnet run --project src/FCG.API
```

As migrations e o seed do admin sao aplicados automaticamente no warmup da aplicacao.

## Principais Endpoints

### Autenticacao

- `POST /api/auth/register`
- `POST /api/auth/login`

### Jogos

- `GET /api/games`
- `GET /api/games/{id}`
- `POST /api/games` - Admin

### Pedidos

- `POST /api/orders`
- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders/{id}/pay`

### Biblioteca

- `GET /api/library`

### Promocoes

- `GET /api/promotions`
- `POST /api/promotions` - Admin
- `DELETE /api/promotions/{id}` - Admin

### Administracao de Usuarios

- `POST /api/admin/users` - Admin
- `GET /api/admin/users` - Admin
- `GET /api/admin/users/{id}` - Admin
- `PATCH /api/admin/users/{id}/role` - Admin
- `DELETE /api/admin/users/{id}` - Admin, inativa usuario, exceto usuario default
- `PATCH /api/admin/users/{id}/reactivate` - Admin, reativa usuario

Exemplo de body para criar usuario Admin:

```json
{
  "name": "Admin 2",
  "email": "admin2@email.com",
  "password": "Adm!n123",
  "role": "Admin"
}
```

Exemplo de body para alterar role:

```json
{
  "role": "Admin"
}
```

## Testes

```bash
dotnet test
```

Para executar com cobertura das camadas de regras de negocio:

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

Os testes cobrem validacao de senha/e-mail, politica de senha forte, regras de dominio de pedidos, autorizacao por dono do pedido, fluxo de pedidos, promocoes, catalogo, administracao de usuarios e inativacao logica.
