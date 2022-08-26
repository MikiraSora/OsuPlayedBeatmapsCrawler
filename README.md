# OsuPlayedBeatmapsCrawler
一个osu!爬虫工具，可以爬取你所有玩过的谱面并批量下载

## 特别感谢
* osu!官网  ， 程序爬虫需要从官网爬取你的所有游戏记录和对应的谱面元数据
* [sayo小夜谱面镜像站](https://osu.sayobot.cn/home)  ，  程序从这里批量下载osz谱面文件

## 食用方式
1. 首先你有一个osu账号，并且还是[撒泼特](https://osu.ppy.sh/home/support)
2. 打开命令提示行
3. 切换文件夹到本爬虫工具的文件夹
4. 执行以下命令:
```bash
.\OsuPlayedBeatmapsCrawler.exe --username '用户名' --password '密码' --save 'D:\Beatmaps'
```
5. 挂机

## 工具命令
```bash
  --secert    program will load this file and get username and password //如果不方便直使用--password/--username参数,那么可以使用此参数间接读取用户名和密码。这参数将读取指定文件的内容，文件第一行为用户名，第二行为用户密码。

  --username    osu! user name //用户名

  --password    osu! user password //密码

  --cursor      (Default: ) [option] cursor string //每次osu查询谱面都会有个cursor字段表示你查询的位置，保存/设置这个字段方便下次直接从这个搜寻位置继续爬数据,如果不设置，则表示从头开始爬

  --save        (Default: ./beatmaps) [option] folder path which saves .osz beatmap files //爬虫下载.osz文件存放的文件夹位置

  --log         (Default: ./logs) [option] folder path which saves program log files //程序日志存放的文件夹位置

  --help        Display this help screen.

  --version     Display version information.
```

## 中断续爬
特殊情况可能应用因为各种原因中止爬取数据，但你可以通过本应用的日志文件提取cursor字段,然后通过设置`--cursor`执行应用可以实现继续爬取数据的功能。<br/>
在日志xxxx.log文件中:
```
crawler download progress : 1.1551983 (GOOD : 46 / BAD : 0 / TOTAL : 3982)
5708       IOSYS                           -  Cirno Chirumiru
downloaded beatmap id : 5708
crawler download progress : 1.1803114 (GOOD : 47 / BAD : 0 / TOTAL : 3982)
3030       Lucky Star                      -  Motteke! Sailor Fuku (REDALiCE Remix)
downloaded beatmap id : 3030
crawler download progress : 1.2054244 (GOOD : 48 / BAD : 0 / TOTAL : 3982)
5192       DM Ashura feat. Tiger Yamato    -  R3 (Omega Mix)
downloaded beatmap id : 5192
crawler download progress : 1.2305374 (GOOD : 49 / BAD : 0 / TOTAL : 3982)
3198       Rhapsody                        -  Emerald Sword
downloaded beatmap id : 3198
crawler download progress : 1.2556504 (GOOD : 50 / BAD : 0 / TOTAL : 3982)
cursor change eyJhcHByb3ZlZF9kYXRlIjoiMTI1MzUwNDU2NDAwMCIsImlkIjoiOTEzMiJ9 -> eyJhcHByb3ZlZF9kYXRlIjoiMTIzOTMzMzU4MzAwMCIsImlkIjoiMzE5OCJ9
3688       Tarou                           -  Danjo
downloaded beatmap id : 3688
crawler download progress : 1.2807634 (GOOD : 51 / BAD : 0 / TOTAL : 3982)
6257       Yoko Hikasa                     -  Don't Say "Lazy"
```
1. 找到最后出现`cursor change`内容那一行
2. 复制→右边的内容,即类似`eyJhcHByb3ZlZF9kYXRlIjoiMTIzOTMzMzU4MzAwMCIsImlkIjoiMzE5OCJ9`那一串
3. 命令行执行程序，添加`--cursor`参数，值便是刚才复制那一坨
```bash
.\OsuPlayedBeatmapsCrawler.exe --username 'DarkProjector' --password 'VW50CrazyTHU' --cursor 'eyJhcHByb3ZlZF9kYXRlIjoiMTIzOTMzMzU4MzAwMCIsImlkIjoiMzE5OCJ9'
```
4. 爬虫会重新从指定的位置继续爬取数据&下载



