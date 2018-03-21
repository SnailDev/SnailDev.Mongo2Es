# Mongo2Es

ElasticSearch and MongoDB sync tools base in netcore2

Chinese Documentation - [中文文档](./README.zh-CN.md)

![structure]

Supports one-to-one, one-to-many, many-to-one, and many-to-many relationships.

- **one-to-one** - one mongodb collection to one elasticsearch index
- **one-to-many** - one mongodb collection to many elasticsearch indexs
- **many-to-one** - many mongodb collections to one elasticsearch index
- **many-to-many** - many mongodb collections to many elasticsearch indexs

##  version

    elasticsearch：v6.1.2
    mongodb: v3.4.9
    netcore: v2.1.101

## What does it do

Mongo2Es keeps your mongoDB collections and elastic search cluster in sync. It does so by tailing the mongo oplog and replicate whatever crud operation into elastic search cluster without any overhead. Please note that a replica set is needed for the Mongo2Es to tail mongoDB.

## How to use

[Download](https://github.com/SnailDev/SnailDev.Mongo2Es/tree/master) from GitHub
```bash
cd src
dotnet publish --framework netcoreapp2.0 -o ./published 
```

## Start up

```bash
dotnet Mongo2Es.dll --port {port for web manage} --mongo {mongourl for config}
```

## Result
- **webManage**

![webmanage]

- **processing**

![process1]
![process2]


- **mongodbData**

![mongodb1]
![mongodb2]

- **elasticsearch**

![elasticsearch]


## License

The MIT License (MIT). Please see [LICENSE](LICENSE) for more information.

[structure]:./src/Mongo2Es/wwwroot/images/introduction/structure.jpg "structure"

[webmanage]:./src/Mongo2Es/wwwroot/images/introduction/webmanage.png "webmanage"

[mongodb1]:./src/Mongo2Es/wwwroot/images/introduction/mongodb1.jpg "mongodb1"

[mongodb2]:./src/Mongo2Es/wwwroot/images/introduction/mongodb2.jpg "mongodb2"

[elasticsearch]:./src/Mongo2Es/wwwroot/images/introduction/elasticsearch.jpg "elasticsearch"

[process1]:./src/Mongo2Es/wwwroot/images/introduction/process1.jpg "process1"

[process2]:./src/Mongo2Es/wwwroot/images/introduction/process2.jpg "process2"