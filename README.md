# MyLab.FileStorage (FS)

`MyLab.FS` - файловое хранилище с прямым авторизованным доступом клиента.

Ознакомьтесь с последними изменениями в [журнале изменений](/CHANGELOG.md).

Docker образ `API`: [![Docker image](https://img.shields.io/static/v1?label=docker&style=flat&logo=docker&message=image&color=blue)](https://github.com/orgs/mylab-tools/packages/container/package/fs-api)

Docker образ `Clenaer`: [![Docker image](https://img.shields.io/static/v1?label=docker&style=flat&logo=docker&message=image&color=blue)](https://github.com/orgs/mylab-tools/packages/container/package/fs-cleaner)

Спецификация `API` : [![API specification](https://img.shields.io/badge/OAS3-v1%20-green)](https://app.swaggerhub.com/apis/ozzy/MyLab.FileStorage/1)

Клиент: [![NuGet](https://img.shields.io/nuget/v/MyLab.FileStorage.Client.svg)](https://www.nuget.org/packages/MyLab.FileStorage.Client/)

## Обзор 

`MyLab.FS` (далее *файловое хранилище*) - сервис, предназначенный для обеспечения функций хранения в информационной системе с авторизованным доступом клиентов.

Сервис позволяет загружать, хранить и скачивать файлы. Есть методы доступа с авторизацией для клиентов и без авторизации - для компонентов серверного приложения. 

> *API хранилища не разделено каким либо образом на предназначенное для сервера и - для клиента, т.к. вопросы организации доступа к HTTP-API не является задачей данного сервиса.* 

![](./doc/diagramms/mylab-fs-inner.png)

Файлы хранятся в файловой системе сервера, где развёрнуто *файловое хранилище*. Каждый файл представляет директория, путь к которой строится с использованием идентификатора файла. 

> *Хранилище не обеспечивает резервирования, теневого копирования, кластеризацию и другие средства обеспечения целостности хранящихся данных. Для этих целей используйте сторонние решения.*

Эта директория содержит файлы:

* `content.bin` - содержательная часть файла;
* `metadata.json` - метаданные файла.

Ниже приведён пример метаданных файла:

```json
{
  "id": "94b721e7bbfe4109864dcd8bef70d48e",
  "created": "2001-01-01 21:22:23",
  "md5": "e807f1fcf82d132f9bb018ca6738a19f",
  "filename": "doc.txt",
  "length": 10000,
  "labels": {
    "owner": "user@host.com",
    "sign": "b2xvbG8="
  }
}
```

, где:

* `id` - уникальный идентификатор файла в формате;
* `created` - дата и время появления файла в хранилище;
* `md5` - `MD5` хэш файла в формате `HEX` строки;
* `filename` - имя файла;
* `labels` - произвольные метки в формате ключ-значение, имеющие значение в предметной области приложения.

## Загрузка файла

### Загрузка сервером

При необходимости передать файл клиенту, серверное приложение может загрузить файл в *файловое хранилище* путём межсервисного взаимодействия, используя методы загрузки файла без авторизации (просто указав идентификатор файла) и запросить токен скачивания, который передаст клиенту. 

Загрузка серверным приложением осуществляется без авторизации и состоит из следующих шагов:

* запрашивает токен загрузки;
* отправляет файл частями, прикладывая токен загрузки;
* завершает загрузку, указав контрольную сумму, метаданные и приложив токен загрузки;
* получает метаданные загруженного файла и подписанный токен файла;
* при необходимости проверяет полученные данные;
* подтверждает, что файл получен целевым сервисом;
* применяет данные файла (например, сохраняет идентификатор файла).

![](./doc/diagramms/mylab-fs-trusted-uploading.png)

### Загрузка клиентом

Для загрузки файла клиентом, серверное приложение должно запросить у *файлового хранилища* токен загрузки и передать его этому авторизованному клиенту. Далее клиент загружает файл напрямую в *файловое хранилище*, прикладывая выданный токен. Таким образом право на загрузку файла выдаёт серверное приложение в соответствии со своими бизнес-правилами. Токен загрузки - `JWT` токен с `HMAC-SHA256` подписью.

В результате загрузки файла, клиент получает токен файла - подписанные данные о файле. Токен файла - `JWT` токен с `HMAC-SHA256` подписью. Получив этот токен от клиента, серверное приложение проверяет подпись токена, время его действия, а так же другие реквизиты в соответствии со своим бизнес-правилами. Например, лимит по размеру. В завершении приложение подтверждает в *файловом хранилище* получение файла.

Для проверки подписи токена файла приложение должно разделять секрет с *файловым хранилищем*, которым оно подписывает токены документов. После проверки, серверное приложение применяет полученные данные в соответствии со своим бизнес-процессом.

![](./doc/diagramms/mylab-fs-client-uploading.png)

### Проверка токена файла

Протокол загрузки файла клиентом подразумевает завершающим действием передачу токена файла серверному приложению, для которого загружался файл. Токен файла клиент получает при завершении загрузки файла. 

В свою очередь серверное приложение должно проверить токен перед тем, как применить полученные данные. 

> *В большинстве случаев необходимо сохранить только идентификатор файла. Реже - имя файла, чтобы позже была возможность передавать на клиент, в том числе, и имя для отображения без необходимости обращаться в файловое хранилище за этим именем.* 

Проверка токена файла состоит из следующих шагов:

* **проверка подписи**:

  Токен файла - `JWT`-токен с подписью по алгоритму `HMAC-SHA256`. При формировании подписи используется бинарный ключ длиной не менее 128 бит. В качестве ключа удобно использовать (например, в конфигурации) строку длиной не менее 16 символов. *Файловое хранилище* использует отдельный ключ для подписи токенов файлов. 

* **проверка времени действия**;

* **проверка назначения файла**:

  При начале загрузки, когда серверное приложение по инициативе авторизованного клиента запрашивает токен загрузки у *файлового хранилища*, есть возможность указать **назначение файла** - поле `purpose`. Это произвольное строковое значение, которое характеризует область применения файла. На деле это позволяет приложению-получателю определить правильность адресации загруженного файла. 

  Например, если в приложении есть два сервиса: 

  * визуализация `XML` файла по схеме `XML->HTML->PDF` (`purpose = xml-vis`)
  * отправка сообщений другим пользователям с вложениями (`purpose = msg-att`).

  И клиент загрузил XML файл для визуализации, но токен файла передал в сервис отправки сообщений, указав как токен файла вложения. В этом случае серверное приложение может проверить это значение в токене файла и отказать. 

  Если требуется один и тот же файл использовать для разных целей, необходимо использовать общую цель. Например `xml-vis+mail-att`. Тогда сервисы-получатели могут иметь список действующих целей для проверки, что цель из токена соответствует хотя бы одному варианту. 

* **проверка реквизитов файла**:

  У серверного приложения могут быть и другие требования к файлу. Например, может быть ограничение по размеру файла.

  Кроме того, у файла есть набор меток `labels` в формате ключ-значение. Эти метки могут хранить любую информацию из предметной области прилоэения, в том числе необходимую для проверки загруженного файла. Например, там может быть указана электронная подпись, которую потребуется проверить.

## Скачивание файла

### Скачивание сервером

При необходимости получить файл, серверное приложение может скачать файл из файлового хранилища путём межсервисного взаимодействия, используя методы скачивания файла без авторизации (просто указав идентификатор файла).

Скачивание серверным приложением осуществляется без авторизации и заключается в запросе содержания файла по частям.

![](./doc/diagramms/mylab-fs-trusted-download.png)

### Скачивание клиентом

Для скачивания файла клиентом, серверное приложение должно запросить у *файлового хранилища* токен скачивания и передать его этому авторизованному клиенту. Далее клиент скачивает файл напрямую из *файлового хранилища*, прикладывая выданный токен. Таким образом право на скачивание файла выдаёт серверное приложение в соответствии со своими бизнес-правилами. Токен скачивания - `JWT` токен с `HMAC-SHA256` подписью.

![](./doc/diagramms/mylab-fs-client-downloading.png)

## Конфигурация

Конфигурация файлового хранилища может быть осуществлена как через файл конфигурации, так и через переменные окружения в соответствии со [стандартными механизмами .NET](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0).

Узел в конфигурации - `FS`. Ниже приведён пример конфигурации через переменные окружения:

```
FS__TransferTokenSecret=1234567890123456
FS__FileTokenSecret=6543210987654321
```

Поля конфигурации:

* `Directory` - базовая директория для хранения файлов. По умолчанию - **/var/fs/data**;
* `TransferTokenSecret` - ключ подписи токенов загрузки и скачивания. **Обязательный параметр**; 
* `FileTokenSecret` - ключ подписи токенов файлов. **Обязательный параметр**;
* `UploadTokenTtlSec` - время жизни токена загрузки в секундах. По умолчанию - **1 час**;
* `DownloadTokenTtlSec` - время жизни токена скачивания в секундах. По умолчанию - **1 час**;
* `FileTokenTtlSec` - время жизни токена файла в секундах. По умолчанию - **1 час**;
* `UploadChunkLimitKiB` - максимальный размер загружаемой части файла в `KiB`. По умолчанию - **0.5 MiB**;
* `DownloadChunkLimitKiB` - максимальный размер скачиваемой части файла в `KiB`. По умолчанию - **0.5 MiB**;
* `StoredFileSizeLimitMiB` - максимальный размер файла для хранения в `MiB`. По умолчанию - **нет ограничения**.

Ключ подписи (`TransferTokenSecret` и `FileTokenSecret`) - строка с произвольным текстом. Длина строки должна быть больше 16 символов. Приложение конвертирует эту строку в байты по кодировке `UTF-8` и использует получившийся бинарный ключ для подписи токенов.

## Cleaner

### Обзор

`Cleaner` - приложение - задача. Осуществляет чистку файлового хранилища. Активируется по `http`-запросу:

```http
POST /processing
```

Целевое использование - в паре с планировщиком задач.

Очистка хранилища - удаление устаревших файлов:

* если файл не подтверждён и создан ранее указанного в конфигурации периода относительно текущего времени;
* если файл подтверждён и создан ранее указанного в конфигурации периода относительно текущего времени.

Удаление подтверждённых файлов опционально и настраивается в конфигурации. Используется для работы *файлового хранилища* в режиме `хранилища временных файлов`.

### Конфигурация

Узел конфигурации - `Cleaner`.

Параметры конфигурации:

* `Directory` - базовая директория для хранения файлов. По умолчанию - **/var/fs/data**;
* `LostFileTtlHours` - время жизни неподтверждённых файлов в часах. 1 час по умолчанию;
* `StoredFileTtlHours` - время жизни подтверждённых файлов в часах. Неограниченно по умолчанию.

Пример конфигурации через переменные окружения:

```
Cleaner__Directory=/var/fs/data
Cleaner__LostFileTtlHours=1
Cleaner__StoredFileTtlHours=12
```

## Развёртывание

В данном разделе рассмотрено развёртывание с использованием `docker` контейнеров.

Ниже приведён пример `docker-compose.yml` для развёртывания *файлового хранилища*:

```yaml
version: '3.2'

services:
  mylab-fs-api:
    container_name: mylab-fs-api
    image: ghcr.io/mylab-tools/fs-api:latest
    volumes:
    - fs_data: /var/fs/data
    environment:
    - FS__TransferTokenSecret=1234567890123456
    - FS__FileTokenSecret=6543210987654321
    
  mylab-fs-cleaner:
    container_name: mylab-fs-cleaner
    image: ghcr.io/mylab-tools/fs-cleaner:latest
    volumes:
    - fs_data: /var/fs/data
    environment:
    - Cleaner__LostFileTtlHours=1
    
volumes:
  fs_data:
```

## Клиент

Для разработки клиента на `.NET` предусмотрена библиотека с контрактами `API` сервиса *файлового хранилища*. Опубликовано в виде `NuGet` пакета [MyLab.FileStorage.Client](https://www.nuget.org/packages/MyLab.FileStorage.Client/). 

Контракты разработаны с использованием [MyLab.ApiClient](https://github.com/mylab-tools/apiclient):

* [`IFsFilesApiV1`](./src/MyLAb.FileStorage.Client/IFsFilesApiV1.cs) - API доступа к фалам. Ключ конфигурации `fs-files`;
* [`IFsDownloadApiV1`](./src/MyLAb.FileStorage.Client/IFsDownloadApiV1.cs) - API Скачивания. Ключ конфигурации `fs-download`;
* [`IFsUploadApiV1`](./src/MyLAb.FileStorage.Client/IFsUploadApiV1.cs) - API Загрузки. Ключ конфигурации `fs-upload`.
