# EduCenter

## Локальный запуск API с JWT

Для запуска API нужен ключ подписи JWT `Jwt:SigningKey`. Срок жизни access token — 30 минут.

Из корня репозитория в PowerShell:

```powershell
dotnet user-secrets init --project EduCenter.Api
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $keyBytes = New-Object byte[] 32
    $rng.GetBytes($keyBytes)
    $jwtKey = [Convert]::ToBase64String($keyBytes)
    dotnet user-secrets set "Jwt:SigningKey" $jwtKey --project EduCenter.Api
} finally {
    $rng.Dispose()
    Remove-Variable rng, keyBytes, jwtKey -ErrorAction SilentlyContinue
}
```

Для окружения без User Secrets задайте переменную `Jwt__SigningKey` в конфигурации среды. Используйте случайный ключ не короче 32 байт и храните его в менеджере секретов.

Запустите API:

```powershell
dotnet run --project EduCenter.Api
```

Swagger UI доступен по адресу `/swagger`. Выполните `POST /api/auth/login` с одной из демонстрационных учётных записей:

```json
{
  "username": "graduate-demo",
  "password": "GraduateDemo!2026"
}
```
ИЛИ
```json
{
  "username": "student-demo",
  "password": "StudentDemo!2026"
}
```

Скопируйте `accessToken` из ответа, нажмите **Authorize** в Swagger UI и вставьте токен. После этого можно вызывать защищённый `POST /api/cart-price`. Вход в систему остаётся анонимным и не требует Bearer-токена.
