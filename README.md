# TestTask
Программа скачивает в файл contains.txt (на рабочий стол)  текстовое содержимое введенного сайта. 
Подсчитывает кол-во уникальных слов и сохраняет результат в файл result.txt (на рабочий стол).
После чего данные из файла result.txt импортируются в базу данных SQL. Если данные по этому сайту там уже были, то таблица удаляется и создается новая.
Путь подключения к базе данных прописывается в файле app.config

Программа протестирована на сайтах yandex.ru, rambler.ru, mail.ru.
Так же протестирована на отсутствие подключения к интертнет, отсутсвие файлов сохранения и результата, на ошибку подключения к базе данных, на проверку формата ввода адреса сайта.
