# Лог изменений

Все заметные изменения в этом проекте будут отражаться в этом документе.

Формат лога изменений базируется на [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.3.2] - 2023-01-10

### Добавлено

* предоставление http-метрик.

## [1.2.2] - 2022-12-14

### Изменено

* запрос нового файла стал опциональным
* указание назначения файла стало опциональным

## [1.1.2] - 2022-12-14

### Добавлено

* тесты

### Изменено

* документация в части загрузки файлов

## [1.0.2] - 2022-12-12

### Исправлено

* неправильное добавление сервисов в приложение;
* неправильный базовый образ для `API` и `Cleaner`;
* не указаны ключи конфигурации для контрактов;
* проблемы с промышленной реализацией `FileStorageOperator`;
* проблема скачивания;

### Добавлено

* `404(NotFound)` при получении метаданных файла, если файл не подтверждён;
* `409(Conflict)` при подтверждении, если файл уже подтверждён. 
