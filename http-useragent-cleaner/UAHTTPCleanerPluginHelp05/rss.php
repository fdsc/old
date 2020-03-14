<?php
header('Content-Type: application/rss+xml');
// Управление кешем в .htaccess
// header('Cache-Control: max-age=0, must-revalidate, proxy-revalidate, no-cache, no-store, private');

echo '<?xml version="1.0" encoding="utf-8"?>';

$a = $_SERVER['HTTP_ACCEPT_LANGUAGE'];
$i = strpos($a, 'RU');
?>

<rss version="2.0"  xmlns:atom="http://www.w3.org/2005/Atom">
<channel>
<atom:link href="http://huac.8vs.ru/rss.php" rel="self" type="application/rss+xml" />
<link>http://huac.8vs.ru/</link>
<?php
if ($i === FALSE)
{
 ?>
 <title>huac.8vs.ru news (in Russian language)</title>
<description>huac.8vs.ru news</description>
<?php
}
else
{
?>
<title>Новости huac.8vs.ru</title>
<description>Новости huac.8vs.ru</description>
<?php
}
?>

<lastBuildDate>Sun, 14 Oct 2018 19:30:10 GMT</lastBuildDate>

<item>
	<title>Версия 2.3.5-b00</title>
	<description>На вкладку "HTTP" добавлен фильтр "ClientRects", вносящий искажения в работу функции Element.GetClientRects. Будьте осторожны, функция сбивает параметры отображения и может сдвинуть html-элементы на странице.
	</description>
	<category>update</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Sun, 14 Oct 2018 19:30:10 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201810141</guid>
</item>

<item>
	<title>Версия 2.3.4-b07</title>
	<description>Для фильтра JS вкладки "Сторонние" добавлено отображение содержимого элемента noscript. На некоторых сайтах, таких как duckduckgo это помогает пользоваться сайтом без включения JavaScript. Если вы ещё не прошли опрос пользователей https://connect.yandex.ru/forms/5bace71e264d25004a688e85/ , пожалуйста, примите участие.
	Пожалуйста, убедитесь, что rss-лента проверяется не чаще, чем раз в 24 часа.
	</description>
	<category>update</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Sun, 14 Oct 2018 15:05:05 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201810140</guid>
</item>


<item>
	<title>Опрос пользователей</title>
	<description>Опрос пользователей https://connect.yandex.ru/forms/5bace71e264d25004a688e85/ . Пожалуйста, примите участие.
	</description>
	<category>update</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Thu, 27 Sep 2018 16:41:25 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201809270</guid>
</item>

<item>
	<title>Фильтр JS</title>
	<description>В новой версии 2.3.4-b01 для браузера Basilisk добавлен фильтр JS (вкладка "Сторонние"). JS:+ будет блокировать все скрипты на странице с помощью вставки дополнительного заголовка Content-Security-Policy. Это несколько неудобно, так как блокируются сразу все скрипты и консоль браузера засоряется сообщениями о блокировках, однако позволяет найти простую замену NoScript, если по каким-то причинам старая версия NoScript недоступна.
	Так же напоминаю, что настройки Http UserAgent Cleaner со старой версии FireFox в Basilisk можно перенести путём копирования файлов настроек (см. главную страницу дополнения).
	</description>
	<category>update</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Thu, 23 Nov 2017 19:49:18 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201711230</guid>
</item>

<item>
	<title>Basilisk</title>
	<description>На браузере Basilisk https://www.basilisk-browser.org пока существуют проблемы с обработкой некоторых правил. В частности, не работает phase[:]=CP для страниц (документов верхнего уровня), для них же не работает условие PRIVATE на фазе content-policy (CP). Однако, видимо, разработчики браузера Basilisk будут так любезны, что в следующем релизе проблема, внесённая в FireFox командой Mozilla будет устранена в Basilisk командой PaleMoon. Остаётся только надеяться на то, что так и будет.
	</description>
	<category>alert</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Sun, 19 Nov 2017 17:04:17 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201711190</guid>
</item>

<item>
	<title>Переход на браузер Basilisk</title>
	<description>Дополнение работает и поддерживается на браузере Basilisk https://www.basilisk-browser.org . FireFox больше не поддерживается.
	</description>
	<category>alert</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Sat, 18 Nov 2017 10:53:29 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201711180</guid>
</item>

<item>
	<title>По техническим причинам сайт дополнения теперь huac.8vs.ru</title>
	<description>По техническим причинам сайт дополнения теперь huac.8vs.ru . Пожалуйста, измените ваши ссылки на ленту новостей.
	</description>
	<category>alert</category>
	<link>http://huac.8vs.ru/news.html</link>
	<pubDate>Thu, 23 Feb 2017 07:25:53 GMT</pubDate>
	<guid>http://huac.8vs.ru/rss.php?n=201702231</guid>
</item>

</channel>
</rss>

<?php

$f = fopen('./stat/statrss' . date('Y-m-d') . '.log', 'a');
fwrite($f, date('H:i:s') . ' ' . $a . "\r\n");
fclose($f);

?>
