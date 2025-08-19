# Запуск
## Visual Studio
Перед **первым** запуском проекта, в Package Manager Console выполните команду для проекта `Api`:
```powershell
PM> Add-Migration Init
```

Далее необходимо выбрать в вернхей панеле вариант запуска `docker-compose` слева от кнопки запуска приложения.

## Консоль
В корневой директории, где расположен файл `docker-compose.yml` в консоли выполнить команды:
```powershell
..> dotnet ef migrations add Init --project Api/ --startup-project Api/Api.csproj
..> docker-compose up --build -d
```
___
> **API банка** станет доступен по адресу: `http://localhost:80/`\
> **Сервис для аутентификации и получаения доступа** к API станет доступен по адресу: `http://localhost:5210/`\
> **Hangfire** станет доступен по адресу: `http://localhost:5183/`\
> **RabbitMQ** станет доступен по адресу: `http://localhost:15672/` (логин:пароль admin:admin)

**NB: Очереди, биндинги и так далее автоматически загружаются в RabbitMQ сервис при запуске docker-compose. Аналогично, автоматически загружается plpgsql процедура по начислению процентов.**

# Выполнение операций
В БД, которая автоматически будет создана, будет находиться 3 сущности `Client`: менеджер Анна, кассир Алексей и клиент Иван

## Авторизация (получение доступа функциям сервиса)

1. Выполнить POST запрос по адресу: `http://localhost:5210/login` с телом запроса:
```json
{
    "name": "somename",
    "secret": "this-is-should-be-pretty-secure-secret-key",
    "issuer": "BankAccountsAuthApi",
    "audience": "BankAccountsApi"
}
```
> Параметр `secret` в данный момент размещен грубым способ в коде проекта: `Api/Program.cs`.

В полученном ответе нужно сохранить значение поля `token`.

2. Для каждого запроса к эндпоинтам api банка необходимо будет добавлять хедер со значением сохраненного `token`:

```
Authorization: Bearer <jwt...>
```

> Данный токен существует 30 минут.

## Функции сервиса
- создать счёт (`AccountsController.CreateAccount`)
- изменить счёт (`AccountsController.UpdateAccount`)
- удалить счёт (`AccountsController.DeleteAccount`)
- получить список счетов (`AccountsController.GetAccounts`)
- зарегистрировать транзакцию по счёту (`TransactionsController.CreateTransaction`)
- выполнить перевод между счётами (`AccountsController.Transfer`)
- выдать выписку клиенту (`AccountsController.GetStatement`)
- проверить наличие счёта у клиента (`AccountsController.ClientHasAccount`)

## Выполненией сценариев
> Для получения `ownerId` (id клиента) выполнить `GET` запрос по адресу `http://localhost:5239/api/clients`.

***Сценарий 1. Я, как менеджер банка Анна, открыла клиенту Ивану бесплатный текущий счёт, чтобы он мог хранить средства.***

Выполнить `POST` запрос по адресу `http://localhost:5239/api/accounts` с телом запроса:
```json
{
  "ownerId": "07c7d13d-555c-4aee-8902-c2760101535a",
  "type": 0,
  "currency": "RUB",
  "balance": 0,
  "interestRate": 0
}
```

***Сценарий 2. Я, как менеджер банка Анна, открыла клиенту Ивану срочный вклад «Надёжный‑6» под 3 % годовых, чтобы он смог накопить средства.***

Выполнить `POST` запрос по адресу `http://localhost:5239/api/accounts` с телом запроса:
```json
{
  "ownerId": "07c7d13d-555c-4aee-8902-c2760101535a",
  "type": 1,
  "currency": "RUB",
  "balance": 0,
  "interestRate": 3
}
```

***Сценарий 3. Я, как кассир банка Алексей, пополнил текущий счёт клиента Ивана на 1 000 рублей наличными.***

Выполнить `POST` запрос по адресу `http://localhost:5239/api/transactions` с телом запроса:
```json
{
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 1000,
  "currency": "RUB",
  "type": 1
}
```
> Т.к. наличные не имеют исходящего счета, то в теле задействован только `accoutnId`, без `counterPartyAccountId`

***Сценарий 4. Я, как клиент банка Иван, перевёл 200 рублей со своего текущего счёта на вклад «Надёжный‑6», чтобы пополнить вклад.***

Выполнить `POST` запрос по адресу `http://localhost:5239/api/accounts/{senderAccountId}/transfer` с телом запроса:
```json
{
  "recipientId": "1f3be8c7-caac-441e-bf2f-596dee2010bc",
  "amount": 500
}
```
