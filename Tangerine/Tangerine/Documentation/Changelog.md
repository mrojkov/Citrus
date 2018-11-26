##### Changelog 26.11.2018

* Исправлена ошибка, приводящая к крашу, если была использована модель .fbx c частотой кадров 30 fps [CIT-947](https://gitlab.game-forest.com:2000/browse/CIT-947)

##### Changelog 22.11.2018

* Исправлена ошибка в SplineGear, приводящая к неверному просчету анимации при использовании AlongPathOrientation [CIT-905](https://gitlab.game-forest.com:2000/browse/CIT-905)
* Исправлена ошибка, позволяющая обходить валидацию имени виджетов через переименование в списке Timeline [CIT-906](https://gitlab.game-forest.com:2000/browse/CIT-906)
* Исправлен краш при установке ключа анимации хоткеем при развернутом списке аниматоров [CIT-918](https://gitlab.game-forest.com:2000/browse/CIT-918)
* Исправлена ошибка, приводящая к применению всех доступных анимаций родительского виджета к дочерним, когда этого не требовалось [CIT-925](https://gitlab.game-forest.com:2000/browse/CIT-925)

##### Changelog 15.11.2018

* Теперь позиция скролла в Inspector сохраняется при выборе однотипного виджета [CIT-882](https://gitlab.game-forest.com:2000/browse/CIT-882)
* Реализован проброс параметра `tag` в `TaskList.AddLoop` [CIT-904](https://gitlab.game-forest.com:2000/browse/CIT-904)

Багфиксы:

* Исправлена ошибка, позволяющая создать виджет с отрицательными значениями параметра Size [CIT-896](https://gitlab.game-forest.com:2000/browse/CIT-896)
* Исправлена ошибка, не позволяющая возвращать исходные значения параметра (после Undo), если они были изменены через Paste Keyframes [CIT-903](https://gitlab.game-forest.com:2000/browse/CIT-903)

##### Changelog 14.11.2018
* Добавлена документация по редактору. Теперь узнать информацию по Tangerine можно, вызвав Help -> View Help `Ctrl + F1` (на данный момент работает только на платформе Win) [CIT-798](https://gitlab.game-forest.com:2000/browse/CIT-798)

Багфиксы:

* Исправлена ошибка, создающая в дочерних виджетах лейаутов LayoutConstraints, если у виджета уже был добавлен кастомный компонент, наследуемый от класса LayoutConstraints [CIT-752](https://gitlab.game-forest.com:2000/browse/CIT-752)
* Исправлена ошибка, приводящая к невозможности смены типа интерполяции у множественного выделения виджетов [CIT-848](https://gitlab.game-forest.com:2000/browse/CIT-848)
* Исправлена регрессия, не позволяющая скроллить список виджетов таймлайна при перетаскивании виджетов [CIT-881](https://gitlab.game-forest.com:2000/browse/CIT-881)

##### Changelog (12.11.2018 - 13.11.2018)

* Разворачиваемые списки больше не сворачиваются при смене кадра [CIT-764](https://gitlab.game-forest.com:2000/browse/CIT-764)
* Теперь по клику в любое доступное место можно снять фокус с текущего активного текстового/числового поля [CIT-792](https://gitlab.game-forest.com:2000/browse/CIT-792)
* MFDecoder убран из движка в 3rdparty [CIT-879](https://gitlab.game-forest.com:2000/browse/CIT-879)

##### Changelog (01.11.2018 - 9.11.2018)

* Переработан интерфейс работы с Attachment3D. Теперь он вызывается как панель (View -> Panels -> Model3D Attachment) [CIT-744](https://gitlab.game-forest.com:2000/browse/CIT-744)
* Добавлен выбор источника анимации при использовании Model3D.
По-умолчанию теперь будут импортироваться все анимации из fbx модели (раньше импортировалась только одна). Следовательно, можно создавать разные анимации вместо размещения всех анимаций на одном таймлайне. В 3DAttachment можно явно указать, какую анимацию нужно перегрузить [CIT-795](https://gitlab.game-forest.com:2000/browse/CIT-795)

Багфиксы:

* Исправлена ошибка, позволяющая при анимировании поля Text создавать новые ключи анимации без Automatic Keyframes [CIT-763](https://gitlab.game-forest.com:2000/browse/CIT-763)
* Исправлена ошибка, приводящая к крашу, если применить Fit to Container при выставленном ключе анимации Rotation [CIT-786](https://gitlab.game-forest.com:2000/browse/CIT-786)
* Исправлена ошибка, приводящая к невозможности использовать стандартные операции (вставка/копирование/вырезание и т.п.) над аниматорами кастомных виджетов [CIT-864](https://gitlab.game-forest.com:2000/browse/CIT-864) / [CIT-887](https://gitlab.game-forest.com:2000/browse/CIT-887)
* Исправлена ошибка, приводившая к крашу при копировании виджета с анимированным Spacing в любом Layout [CIT-886](https://gitlab.game-forest.com:2000/browse/CIT-886)
* Исправлена ошибка, приводивщая к пробрасыванию нажатия Enter в диалоге создания маркера [CIT-891](https://gitlab.game-forest.com:2000/browse/CIT-891)

##### Changelog (1.10.2018 - 1.11.2018)
* Ревамп внешнего вида редактора, изменен дизайн табов панелей и толщина сплиттеров между ними [CIT-332](https://gitlab.game-forest.com:2000/browse/CIT-332)
* Была изменена логика работы с пивотами - теперь активный (выбранный) пивот всегда отображается поверх остальных, также добавлена цветовая индикация. [CIT-810](https://gitlab.game-forest.com:2000/browse/CIT-810)
* Убрана задержка при драге виджетов (теперь она справедлива только для дублирования через Alt) [CIT-819](https://gitlab.game-forest.com:2000/browse/CIT-819)
* Меню, которые невозможно использовать в данный момент, теперь остаются неактивными [CIT-850](https://gitlab.game-forest.com:2000/browse/CIT-850)
* Начат переход к использованию параллельных анимаций. Подробная информация об этом и как это использовать будет позднее. Кратко: теперь есть возможность создавать несколько неймспейсов у корневых виджетов (контейнеров), в пределах этих неймспейсов будут свои ключи анимации и маркеры. Применятся они, соответственно, будут на дочерние контейнеру виджеты
* Исправлено множество ошибок и багов. В том числе устранена проблема с некорректной обработкой даблкликов и большинство проблем с инпутом

##### Changelog (11.09.2018 - 1.10.2018)
* Теперь Orange Launcher регенерирует бинарные десериалайзеры Yuzu [CIT-110](https://gitlab.game-forest.com:2000/browse/CIT-110)
* Добавлена обводка для маркеров на таймлайне [CIT-713](https://gitlab.game-forest.com:2000/browse/CIT-713)
* Добавлена возможность работы с аниматорами на таймлайне [CIT-720](https://gitlab.game-forest.com:2000/browse/CIT-720)
* Реализован ListPropertyEditor (редактор полей типа List<T>) [CIT-750](https://gitlab.game-forest.com:2000/browse/CIT-750)
* Для текста теперь можно указать PathPropertyEditor [CIT-765](https://gitlab.game-forest.com:2000/browse/CIT-765)
* Недостающие цвета в конфиге теперь берутся из текущей темы [CIT-766](https://gitlab.game-forest.com:2000/browse/CIT-766)
* При запуске редактора неиспользуемые сцены не грузятся до их открытия [CIT-790](https://gitlab.game-forest.com:2000/browse/CIT-790)
* Изменена индикация изменений на сцене [CIT-796](https://gitlab.game-forest.com:2000/browse/CIT-796)
* Исправлено множество багов и недоработок

##### Changelog (1.08.2018 - 11.09.2018)
* Добавлен пользовательский компонент Gradient [CIT-102](https://gitlab.game-forest.com:2000/browse/CIT-102)
* Добавлен пользовательский компонент HSL [CIT-650](https://gitlab.game-forest.com:2000/browse/CIT-650)
* Добавлен пользовательский компонент Layout - Stack, Table, Flow, Linear. [layout branch report](https://gitlab.game-forest.com:8888/snippets/13)
* Добавлен фильтр элементов для Viewport (Visual Hints) [CIT-246](https://gitlab.game-forest.com:2000/browse/CIT-246)
* Добавлена индикация связей костей/SplineGear с виджетами [CIT-267](https://gitlab.game-forest.com:2000/browse/CIT-267), [CIT-612](https://gitlab.game-forest.com:2000/browse/CIT-612)
* Добавлена возможность раскрывать список аниматоров хоткеем (`Shift + Space`) [CIT-274](https://gitlab.game-forest.com:2000/browse/CIT-274)
* Добавлена возможность выставить/убрать ключи анимации хоткеями (Position, Scale, Rotation - `E/R/T`) [CIT-280](https://gitlab.game-forest.com:2000/browse/CIT-280), [CIT-673](https://gitlab.game-forest.com:2000/browse/CIT-673)
* Добавлена возможность Copy/Paste в курсор мыши [CIT-286](https://gitlab.game-forest.com:2000/browse/CIT-286)
* Добавлена возможность производить поиск по полю Text [CIT-344](https://gitlab.game-forest.com:2000/browse/CIT-344)
* Добавлен общий заголовок для панелей [CIT-349](https://gitlab.game-forest.com:2000/browse/CIT-349)
* Добавлена возможность конвертировать Frame в Button [CIT-379](https://gitlab.game-forest.com:2000/browse/CIT-379)
* Переработана работа с цветовыми схемами в редакторе [CIT-417](https://gitlab.game-forest.com:2000/browse/CIT-417)
* Добавлена поддержка арифметический операций в числовых полях [CIT-431](https://gitlab.game-forest.com:2000/browse/CIT-431)
* Добавлена возможность смещать/масштабировать ключи анимации (Numeric Scale/Move Keys) [CIT-450](https://gitlab.game-forest.com:2000/browse/CIT-450)
* Исправлено отображение мелких записей в интерфейсе [CIT-466](https://gitlab.game-forest.com:2000/browse/CIT-466)
* Добавлена возможность центрировать экран на выбранном виджете/ноде (Center View) [CIT-490](https://gitlab.game-forest.com:2000/browse/CIT-490)
* Превью виджетов по `Tab` теперь работает циклически [CIT-495](https://gitlab.game-forest.com:2000/browse/CIT-495)
* Окна панелей теперь имеют класс ToolWindow [CIT-496](https://gitlab.game-forest.com:2000/browse/CIT-496)
* Добавлен механизм проверки "плохих" путей файлов [CIT-500](https://gitlab.game-forest.com:2000/browse/CIT-500)
* Переработана панель инструментов (теперь ее можно кастомизировать) [CIT-501](https://gitlab.game-forest.com:2000/browse/CIT-501)
* Добавлена возможность быстрого масштабирования ключей анимации на таймлайне (растягиванием выделения) [CIT-525](https://gitlab.game-forest.com:2000/browse/CIT-525)
* Переработана панель Filesystem с поддержкой адресной строки и навигации по клавишам (как в Windows Explorer) [CIT-534](https://gitlab.game-forest.com:2000/browse/CIT-534)
* Добавлена возможность центрироваться на каретке по хоткею (`Ctrl + Shift + C`) [CIT-539](https://gitlab.game-forest.com:2000/browse/CIT-539)
* Добавлено превью в Filesystem для .tan [CIT-556](https://gitlab.game-forest.com:2000/browse/CIT-556)
* Ускорена работа с ключами анимации/маркерами на таймлайне [CIT-557](https://gitlab.game-forest.com:2000/browse/CIT-557)
* Реализован новый виджет - TiledImage [CIT-560](https://gitlab.game-forest.com:2000/browse/CIT-560)
* Добавлена возможность drag & drop для TiledImage [CIT-625](https://gitlab.game-forest.com:2000/browse/CIT-625)
* Добавлена возможность выставить свой коэф. Zoom [CIT-581](https://gitlab.game-forest.com:2000/browse/CIT-581)
* Добавлена возможность изменения рамок виджета без затрагивания его содержимого [CIT-589](https://gitlab.game-forest.com:2000/browse/CIT-589)
* Реализован AlongPathOrientation для SplineGear [CIT-607](https://gitlab.game-forest.com:2000/browse/CIT-607)
* Переработана панель Search - переименована в Hierarchy, реализовано в виде древовидной структуры [CIT-619](https://gitlab.game-forest.com:2000/browse/CIT-619)
* Удалена возможность анимировать Root-cцену [CIT-613](https://gitlab.game-forest.com:2000/browse/CIT-613)
* Добавлен режим Slowmotion (x0.10) для режима проигрывания анимации (F5) [CIT-629](https://gitlab.game-forest.com:2000/browse/CIT-629)
* Реализован базовый функционал внутренней справки [CIT-634](https://gitlab.game-forest.com:2000/browse/CIT-634)
* Переработан редактор текста для RichText/SimpleText [CIT-641](https://gitlab.game-forest.com:2000/browse/CIT-641)
* Переработан внешний вид меню (добавлены иконки и сплиттеры) [CIT-662](https://gitlab.game-forest.com:2000/browse/CIT-662)
* Unsample animation twice теперь работает только внутри контейнера [CIT-663](https://gitlab.game-forest.com:2000/browse/CIT-663)
* Добавлена возможность выбрать все ключи анимации для текущей строки (`Ctrl + Shift + A`) и выбрать только ключи анимации выделением через `Alt ` [CIT-668](https://gitlab.game-forest.com:2000/browse/CIT-668)
* Выделение теперь сбрасывается по клику на выделенной области [CIT-670](https://gitlab.game-forest.com:2000/browse/CIT-670)
* Изменен порядок интерполяций по-умолчанию (Linear → Spline → Steep → ClosedSpline) [CIT-671](https://gitlab.game-forest.com:2000/browse/CIT-671)
* Добавлена возможность скрывать только выделенные виджеты на таймлайне (Shift + Show widgets) [CIT-675](https://gitlab.game-forest.com:2000/browse/CIT-675)
* Реализована возможность добавить кастомные компоненты в Model3DAttachment [CIT-697](https://gitlab.game-forest.com:2000/browse/CIT-697)
* Добавлена возможность изменять префикс во множественном выделении виджетов [CIT-699](https://gitlab.game-forest.com:2000/browse/CIT-699)
* Реализована функция Timeshift для ParticleEmitter (аналогично HotStudio) [CIT-708](https://gitlab.game-forest.com:2000/browse/CIT-708 )
* Исправлено множество багов и недоработок

##### Changelog (11.08.2018 - 1.10.2018)
* Теперь Orange Launcher регенерирует бинарные десериалайзеры Yuzu [CIT-110](https://gitlab.game-forest.com:2000/browse/CIT-110)
* Добавлена обводка для маркеров на таймлайне [CIT-713](https://gitlab.game-forest.com:2000/browse/CIT-713)
* Добавлена возможность работы с аниматорами на таймлайне [CIT-720](https://gitlab.game-forest.com:2000/browse/CIT-720)
* Реализован ListPropertyEditor (редактор полей типа List<T>) [CIT-750](https://gitlab.game-forest.com:2000/browse/CIT-750)
* Для текста теперь можно указать PathPropertyEditor [CIT-765](https://gitlab.game-forest.com:2000/browse/CIT-765)
* Недостающие цвета в конфиге теперь берутся из текущей темы [CIT-766](https://gitlab.game-forest.com:2000/browse/CIT-766)
* При запуске редактора неиспользуемые сцены не грузятся до их открытия [CIT-790](https://gitlab.game-forest.com:2000/browse/CIT-790)
* Изменена индикация изменений на сцене [CIT-796](https://gitlab.game-forest.com:2000/browse/CIT-796)
* Исправлено множество багов и недоработок
