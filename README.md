# old
old of my code

----
1. http-useragent-cleaner

http-useragent-cleaner outdated code in the "http-useragent-cleaner" directory.

Work only with Basilisk. Builded versions in the directory "http-useragent-cleaner/mozillasigned" and "http-useragent-cleaner/http-useragent-cleaner".

For build need outdated the nodejs "jpm" packet. I don't know if it's possible to build it now. 

-----
2. BlackDisplay

Устаревшая программа для Windows с .NET (теоретически, отдельный билд можно построить для Mono) для напоминания о времени отдыха с функциями шифрования. Обратите внимание, что вычисление хеша sha-3 происходит с ошибкой - вместо него вычисляется оригинальный хеш keccak, а не его модификация sha-3

Для того, чтобы сбилдить верно, нужно помучиться, т.к. в проекте PostBuilder имеются полные пути, зависимые от компьютера. Также в каком-то из проектов могут быть пути в post build actions, но они, кажется, относительные.

Программа содержит ряд ошибок, в том числе, преднамеренно нарушена потокобезопасность: в функциях диалоговых окон для Windows (но не Linux) часть окон отрабатываются не из главного потока приложения. Это может повлечь за собой падение программы. Такие куски убраны из forLinux версии (ищите соответствующие директивы, если хотите сделать код верным и для Windows)
