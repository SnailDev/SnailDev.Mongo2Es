# Mongo2Es

基于netcore实现mongodb和ElasticSearch之间的数据实时同步的工具 
![structure]

支持一对一,一对多,多对一和多对多的数据传输方式.

- **一对一** - 一个mongodb的collection对应一个elasticsearch的index之间的数据同步
- **一对多** - 一个mongodb的collection对应多个elasticsearch的index之间的数据同步
- **多对一** - 多个mongodb的collection对应一个elasticsearch的index之间的数据同步
- **多对多** - 多个mongodb的collection对应多个elasticsearch的index之间的数据同步

##  环境版本

    elasticsearch：v6.1.2
    mongodb: v3.4.9
    netcore: v2.1.101

## 这个工具是干什么的

Mongo2Es是用来保持你的mongoDB collections和你的elasticsearch index之间的数据实时同步.它是用mongo oplog来监听你的mongdb数据是否发生变化,无论是增删改查它都会及时反映到你的elasticsearch index上.在使用本工具之前你必须保证你的mongoDB是符合replica结构的,如果不是请先正确设置之后再使用此工具.

## 如何使用

[Download](https://github.com/SnailDev/SnailDev.Mongo2Es/tree/master) from GitHub
```bash
cd src
dotnet publish --framework netcoreapp2.0 -o ./published 
```

## 如何启动

```bash
dotnet Mongo2Es.dll --port {port for web manage} --mongo {mongourl for config}
```

## 显示的结果
- **页面管理**

![webmanage]

- **执行过程**

![process1]
![process2]


- **mongodb里面的数据**

![mongodb1]
![mongodb2]

- **elasticsearch里面的数据**

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