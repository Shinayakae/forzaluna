using System;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private static void SeedCreditsTranslations()
        {
            AddUiTranslations("Japanese", new[]
            {
                "Teleport routes and community location work.", "テレポートルートとコミュニティの位置作成。",
                "Tuning Reference", "チューニング参考",
                "Select Language", "言語を選択",
                "Live Values", "ライブ値",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "ゲームから現在のクレジット、ホイールスピン、スキルポイントを読み込みます。",
                "Acceleration", "加速",
                "Custom Speed", "カスタム速度",
                "Percentage", "パーセント",
                "Make Default", "既定にする",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "加速モードを1つ選びます。カスタム速度は入力した倍率を、パーセントは微調整を使います。",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "緑で安定飛行が有効になります。SpaceまたはJumpで上昇、Ctrlで下降、W/A/S/Dで走行と旋回、矢印キーで車の位置を微調整（左右で横移動、上下で前後移動）、キーを離すと位置を保持します。",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "オンにすると今すぐ（接続中なら）読み込み、接続のたびに再読み込みします。オフにすると起動が速く安全になります。",
            });
            AddUiTranslations("Chinese", new[]
            {
                "Teleport routes and community location work.", "传送路线与社区位置工作。",
                "Tuning Reference", "调校参考",
                "Select Language", "选择语言",
                "Live Values", "实时数值",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "从游戏读取你当前的点数、转盘和技能点。",
                "Acceleration", "加速",
                "Custom Speed", "自定义速度",
                "Percentage", "百分比",
                "Make Default", "设为默认",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "选择一种加速模式。自定义速度使用输入的倍率；百分比使用微调。",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "绿色启用稳定飞行。Space 或跳跃键上升，Ctrl 下降，W/A/S/D 行驶和转向，方向键微调车辆位置（左右平移，上下前后滑动），松开按键保持位置。",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "开启后会立即读取（若已附加），并在每次附加时重新读取。关闭可让启动更快更安全。",
            });
            AddUiTranslations("Spanish", new[]
            {
                "Teleport routes and community location work.", "Rutas de teletransporte y trabajo de ubicaciones de la comunidad.",
                "Tuning Reference", "Referencia de ajustes",
                "AIO reference", "Referencia AIO",
                "Select Language", "Seleccionar idioma",
                "Live Values", "Valores en vivo",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Lee tus créditos, giros de ruleta y puntos de habilidad actuales del juego.",
                "Acceleration", "Aceleración",
                "Custom Speed", "Velocidad personalizada",
                "Percentage", "Porcentaje",
                "Make Default", "Predeterminar",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Elige un modo de aceleración. Velocidad personalizada usa el multiplicador escrito; Porcentaje usa microajuste.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Verde activa el vuelo estable. Espacio o Salto sube, Ctrl baja, W/A/S/D conduce y gira, las flechas ajustan la posición del auto (izq./der. desplazan de lado, arriba/abajo deslizan), y al soltar las teclas mantiene la posición.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Actívalo para cargarlos ahora (si está conectado) y de nuevo en cada conexión. Desactivado lo omite para un inicio más rápido y seguro.",
            });
            AddUiTranslations("Arabic", new[]
            {
                "Teleport routes and community location work.", "مسارات الانتقال وعمل مواقع المجتمع.",
                "Tuning Reference", "مرجع الضبط",
                "Select Language", "اختر اللغة",
                "Live Values", "القيم المباشرة",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "اقرأ أرصدتك ودورات العجلة ونقاط المهارة الحالية من اللعبة.",
                "Acceleration", "التسارع",
                "Custom Speed", "سرعة مخصصة",
                "Percentage", "النسبة المئوية",
                "Make Default", "تعيين كافتراضي",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "اختر وضع تسارع واحدًا. السرعة المخصصة تستخدم المضاعِف المُدخَل؛ والنسبة المئوية تستخدم ضبطًا دقيقًا.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "الأخضر يفعّل الطيران المستقر. المسافة أو القفز للصعود، Ctrl للنزول، W/A/S/D للقيادة والدوران، مفاتيح الأسهم تعدّل موضع السيارة (يسار/يمين انزلاق جانبي، أعلى/أسفل انزلاق أمامي)، وترك المفاتيح يثبّت الموضع.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "فعّله ليُحمّلها الآن (إن كان مرفقًا) ومجددًا عند كل إرفاق. إيقافه يتخطاها لبدء تشغيل أسرع وأكثر أمانًا.",
            });
            AddUiTranslations("Turkish", new[]
            {
                "Teleport routes and community location work.", "Işınlanma rotaları ve topluluk konum çalışması.",
                "Tuning Reference", "Ayar referansı",
                "Select Language", "Dil Seç",
                "Live Values", "Canlı Değerler",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Mevcut kredilerinizi, çark çevirmelerinizi ve yetenek puanlarınızı oyundan okuyun.",
                "Acceleration", "Hızlanma",
                "Custom Speed", "Özel Hız",
                "Percentage", "Yüzde",
                "Make Default", "Varsayılan Yap",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Bir hızlanma modu seçin. Özel Hız yazılan çarpanı kullanır; Yüzde mikro ayar kullanır.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Yeşil, kararlı uçuşu etkinleştirir. Space veya Zıplama yükselir, Ctrl alçalır, W/A/S/D sürer ve döner, ok tuşları aracın konumunu ayarlar (sol/sağ yana kayma, yukarı/aşağı ileri-geri), tuşları bırakmak konumu sabit tutar.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Şimdi yüklemek için açın (bağlıysa) ve her bağlanmada tekrar yükleyin. Kapalıyken daha hızlı ve güvenli başlatma için atlanır.",
            });
            AddUiTranslations("Polish", new[]
            {
                "Teleport routes and community location work.", "Trasy teleportacji i praca nad lokalizacjami społeczności.",
                "Tuning Reference", "Materiał o tuningu",
                "Select Language", "Wybierz język",
                "Live Values", "Wartości na żywo",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Odczytaj z gry swoje bieżące kredyty, losowania i punkty umiejętności.",
                "Acceleration", "Przyspieszenie",
                "Custom Speed", "Własna prędkość",
                "Percentage", "Procent",
                "Make Default", "Ustaw domyślne",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Wybierz jeden tryb przyspieszenia. Własna prędkość używa wpisanego mnożnika; Procent używa mikroregulacji.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Zielony włącza stabilny lot. Spacja lub Skok wznosi, Ctrl opuszcza, W/A/S/D jedzie i skręca, strzałki przesuwają pozycję auta (lewo/prawo w bok, góra/dół wzdłuż), a puszczenie klawiszy utrzymuje pozycję.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Włącz, aby wczytać je teraz (jeśli podłączono) i ponownie przy każdym podłączeniu. Wyłączone pomija to dla szybszego, bezpieczniejszego startu.",
            });
            AddUiTranslations("German", new[]
            {
                "Teleport routes and community location work.", "Teleport-Routen und Community-Standortarbeit.",
                "Tuning Reference", "Tuning-Referenz",
                "Select Language", "Sprache wählen",
                "Live Values", "Live-Werte",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Liest deine aktuellen Credits, Glücksräder und Fähigkeitspunkte aus dem Spiel.",
                "Acceleration", "Beschleunigung",
                "Custom Speed", "Eigene Geschwindigkeit",
                "Percentage", "Prozent",
                "Make Default", "Als Standard",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Wähle einen Beschleunigungsmodus. Eigene Geschwindigkeit nutzt den eingegebenen Multiplikator; Prozent nutzt Feinabstimmung.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Grün aktiviert den stabilen Flug. Leertaste oder Sprung steigt, Strg sinkt, W/A/S/D fährt und lenkt, die Pfeiltasten verschieben die Position des Autos (links/rechts seitlich, hoch/runter längs), und Loslassen hält die Position.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Einschalten, um sie jetzt (falls verbunden) und bei jedem Verbinden erneut zu laden. Aus überspringt das für einen schnelleren, sichereren Start.",
            });
            AddUiTranslations("Swedish", new[]
            {
                "Teleport routes and community location work.", "Teleportrutter och community-platsarbete.",
                "Tuning Reference", "Trimreferens",
                "Select Language", "Välj språk",
                "Live Values", "Live-värden",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Läs dina aktuella credits, hjulsnurr och färdighetspoäng från spelet.",
                "Acceleration", "Acceleration",
                "Custom Speed", "Anpassad hastighet",
                "Percentage", "Procent",
                "Make Default", "Gör till standard",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Välj ett accelerationsläge. Anpassad hastighet använder den angivna multiplikatorn; Procent använder finjustering.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Grönt aktiverar stabil flygning. Mellanslag eller Hopp stiger, Ctrl sjunker, W/A/S/D kör och svänger, piltangenterna justerar bilens position (vänster/höger i sidled, upp/ner längs riktningen), och att släppa tangenterna håller positionen.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Slå på för att läsa in dem nu (om ansluten) och igen vid varje anslutning. Av hoppar över det för en snabbare, säkrare start.",
            });
            AddUiTranslations("Farsi", new[]
            {
                "Teleport routes and community location work.", "مسیرهای انتقال و کار روی مکان‌های جامعه.",
                "Tuning Reference", "مرجع تنظیمات",
                "Select Language", "انتخاب زبان",
                "Live Values", "مقادیر زنده",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "اعتبار، چرخ‌گردان و امتیاز مهارت فعلی خود را از بازی بخوانید.",
                "Acceleration", "شتاب",
                "Custom Speed", "سرعت سفارشی",
                "Percentage", "درصد",
                "Make Default", "پیش‌فرض کن",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "یک حالت شتاب انتخاب کنید. سرعت سفارشی از ضریب واردشده و درصد از تنظیم دقیق استفاده می‌کند.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "سبز پرواز پایدار را فعال می‌کند. Space یا پرش بالا می‌برد، Ctrl پایین می‌آورد، W/A/S/D می‌راند و می‌چرخاند، کلیدهای جهت موقعیت خودرو را جابه‌جا می‌کنند (چپ/راست جانبی، بالا/پایین در راستای مسیر)، و رها کردن کلیدها موقعیت را نگه می‌دارد.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "روشن کنید تا اکنون (در صورت اتصال) و دوباره در هر اتصال بارگذاری شوند. خاموش برای راه‌اندازی سریع‌تر و ایمن‌تر از آن صرف‌نظر می‌کند.",
            });
            AddUiTranslations("French", new[]
            {
                "Teleport routes and community location work.", "Itinéraires de téléportation et travail sur les lieux communautaires.",
                "Tuning Reference", "Référence de réglages",
                "Select Language", "Choisir la langue",
                "Live Values", "Valeurs en direct",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Lit vos crédits, roues de la fortune et points de compétence actuels depuis le jeu.",
                "Acceleration", "Accélération",
                "Custom Speed", "Vitesse personnalisée",
                "Percentage", "Pourcentage",
                "Make Default", "Par défaut",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Choisissez un mode d'accélération. Vitesse personnalisée utilise le multiplicateur saisi ; Pourcentage utilise un micro-ajustement.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Le vert active le vol stable. Espace ou Saut monte, Ctrl descend, W/A/S/D conduit et tourne, les flèches ajustent la position de la voiture (gauche/droite en latéral, haut/bas en longitudinal), et relâcher les touches maintient la position.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Activez pour les charger maintenant (si connecté) et à chaque connexion. Désactivé, c'est ignoré pour un démarrage plus rapide et sûr.",
            });
            AddUiTranslations("Lithuanian", new[]
            {
                "Teleport routes and community location work.", "Teleportavimo maršrutai ir bendruomenės vietų darbas.",
                "Tuning Reference", "Derinimo žinynas",
                "Select Language", "Pasirinkti kalbą",
                "Live Values", "Tiesioginės reikšmės",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Nuskaitykite iš žaidimo savo dabartinius kreditus, rato sukimus ir įgūdžių taškus.",
                "Acceleration", "Pagreitis",
                "Custom Speed", "Pasirinktinis greitis",
                "Percentage", "Procentai",
                "Make Default", "Padaryti numatytuoju",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Pasirinkite vieną pagreičio režimą. Pasirinktinis greitis naudoja įvestą daugiklį; Procentai naudoja tikslų derinimą.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Žalia įjungia stabilų skrydį. Tarpas arba Šuolis kyla, Ctrl leidžiasi, W/A/S/D važiuoja ir suka, rodyklių klavišai pastumia automobilio padėtį (kairė/dešinė šonu, aukštyn/žemyn išilgai), o atleidus klavišus padėtis išlaikoma.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Įjunkite, kad įkeltumėte juos dabar (jei prijungta) ir vėl po kiekvieno prijungimo. Išjungus praleidžiama greitesnei, saugesnei paleisčiai.",
            });
            AddUiTranslations("Portuguese", new[]
            {
                "Teleport routes and community location work.", "Rotas de teleporte e trabalho de locais da comunidade.",
                "Tuning Reference", "Referência de tuning",
                "Select Language", "Selecionar idioma",
                "Live Values", "Valores ao vivo",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Lê seus créditos, roletas e pontos de habilidade atuais do jogo.",
                "Acceleration", "Aceleração",
                "Custom Speed", "Velocidade personalizada",
                "Percentage", "Porcentagem",
                "Make Default", "Tornar padrão",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Escolha um modo de aceleração. Velocidade personalizada usa o multiplicador digitado; Porcentagem usa microajuste.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Verde ativa o voo estável. Espaço ou Pulo sobe, Ctrl desce, W/A/S/D dirige e vira, as setas ajustam a posição do carro (esq./dir. deslizam de lado, cima/baixo ao longo), e soltar as teclas mantém a posição.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Ative para carregá-los agora (se anexado) e novamente a cada anexação. Desligado pula isso para um início mais rápido e seguro.",
            });
            AddUiTranslations("Indonesian", new[]
            {
                "Teleport routes and community location work.", "Rute teleport dan pengerjaan lokasi komunitas.",
                "Tuning Reference", "Referensi tuning",
                "Select Language", "Pilih Bahasa",
                "Live Values", "Nilai Langsung",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Baca kredit, putaran roda, dan poin keterampilan Anda saat ini dari gim.",
                "Acceleration", "Akselerasi",
                "Custom Speed", "Kecepatan Khusus",
                "Percentage", "Persentase",
                "Make Default", "Jadikan Default",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Pilih satu mode akselerasi. Kecepatan Khusus memakai pengali yang diketik; Persentase memakai penyesuaian mikro.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Hijau mengaktifkan terbang stabil. Spasi atau Lompat naik, Ctrl turun, W/A/S/D mengemudi dan berbelok, tombol panah menggeser posisi mobil (kiri/kanan menyamping, atas/bawah searah), dan melepas tombol menahan posisi.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Aktifkan untuk memuatnya sekarang (jika terpasang) dan lagi setiap kali memasang. Nonaktif melewatinya demi startup yang lebih cepat dan aman.",
            });
            AddUiTranslations("Georgian", new[]
            {
                "Teleport routes and community location work.", "ტელეპორტის მარშრუტები და საზოგადოების მდებარეობების სამუშაო.",
                "Tuning Reference", "ტუნინგის ცნობარი",
                "Select Language", "ენის არჩევა",
                "Live Values", "ცოცხალი მნიშვნელობები",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "წაიკითხეთ თქვენი მიმდინარე კრედიტები, ბორბლის დატრიალებები და უნარის ქულები თამაშიდან.",
                "Acceleration", "აჩქარება",
                "Custom Speed", "მორგებული სიჩქარე",
                "Percentage", "პროცენტი",
                "Make Default", "ნაგულისხმევად დაყენება",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "აირჩიეთ ერთი აჩქარების რეჟიმი. მორგებული სიჩქარე იყენებს აკრეფილ მამრავლს; პროცენტი იყენებს მცირე კორექციას.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "მწვანე რთავს სტაბილურ ფრენას. Space ან ნახტომი ადის, Ctrl ეშვება, W/A/S/D მართავს და უხვევს, ისრის ღილაკები ანაცვლებს მანქანის პოზიციას (მარცხნივ/მარჯვნივ გვერდულად, ზემოთ/ქვემოთ გასწვრივ), ღილაკების აშვება კი ინარჩუნებს პოზიციას.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "ჩართეთ მათ ახლავე ჩასატვირთად (თუ მიერთებულია) და ხელახლა ყოველი მიერთებისას. გამორთვა გამოტოვებს ამას უფრო სწრაფი, უსაფრთხო გაშვებისთვის.",
            });
            AddUiTranslations("Vietnamese", new[]
            {
                "Teleport routes and community location work.", "Tuyến dịch chuyển và công việc vị trí cộng đồng.",
                "Tuning Reference", "Tham khảo tinh chỉnh",
                "Select Language", "Chọn ngôn ngữ",
                "Live Values", "Giá trị trực tiếp",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Đọc tín dụng, vòng quay và điểm kỹ năng hiện tại của bạn từ game.",
                "Acceleration", "Tăng tốc",
                "Custom Speed", "Tốc độ tùy chỉnh",
                "Percentage", "Phần trăm",
                "Make Default", "Đặt mặc định",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Chọn một chế độ tăng tốc. Tốc độ tùy chỉnh dùng hệ số đã nhập; Phần trăm dùng vi chỉnh.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Xanh lá bật bay ổn định. Space hoặc Nhảy để lên, Ctrl để xuống, W/A/S/D lái và rẽ, phím mũi tên dịch vị trí xe (trái/phải trượt ngang, lên/xuống trượt dọc), thả phím sẽ giữ nguyên vị trí.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Bật để tải chúng ngay (nếu đã gắn) và lại sau mỗi lần gắn. Tắt sẽ bỏ qua để khởi động nhanh và an toàn hơn.",
            });
            AddUiTranslations("Dutch", new[]
            {
                "Teleport routes and community location work.", "Teleportroutes en community-locatiewerk.",
                "Tuning Reference", "Tuningreferentie",
                "Database", "Database",
                "Select Language", "Taal selecteren",
                "Live Values", "Live-waarden",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "Leest je huidige credits, rad-draaien en vaardigheidspunten uit het spel.",
                "Acceleration", "Versnelling",
                "Custom Speed", "Aangepaste snelheid",
                "Percentage", "Percentage",
                "Make Default", "Standaard maken",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "Kies één versnellingsmodus. Aangepaste snelheid gebruikt de ingevoerde vermenigvuldiger; Percentage gebruikt microafstelling.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "Groen schakelt stabiel vliegen in. Spatie of Springen stijgt, Ctrl daalt, W/A/S/D rijdt en stuurt, pijltjestoetsen verschuiven de positie van de auto (links/rechts zijwaarts, omhoog/omlaag in de rijrichting), en toetsen loslaten houdt de positie vast.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "Zet aan om ze nu te laden (indien gekoppeld) en opnieuw bij elke koppeling. Uit slaat dit over voor een snellere, veiligere start.",
            });
            AddUiTranslations("Korean", new[]
            {
                "Teleport routes and community location work.", "텔레포트 경로 및 커뮤니티 위치 작업.",
                "Tuning Reference", "튜닝 참고",
                "Thank you for the work, testing, ideas, and support.", "작업, 테스트, 아이디어, 후원에 감사드립니다.",
                "AIO reference", "AIO 참고",
                "Database", "데이터베이스",
                "Emotional Support", "정서적 지원",
                "Select Language", "언어 선택",
                "Live Values", "실시간 값",
                "Read your current Credits, Wheelspins, and Skill Points from the game.", "게임에서 현재 크레딧, 휠스핀, 스킬 포인트를 읽어옵니다.",
                "Acceleration", "가속",
                "Custom Speed", "사용자 지정 속도",
                "Percentage", "백분율",
                "Make Default", "기본값으로 설정",
                "Choose one acceleration mode. Custom Speed uses the typed multiplier; Percentage uses micro adjustment.", "가속 모드를 하나 선택하세요. 사용자 지정 속도는 입력한 배수를, 백분율은 미세 조정을 사용합니다.",
                "Green enables stable flight. Space or Jump rises, Ctrl descends, W/A/S/D drives and turns, arrow keys nudge the car's position (left/right strafe, up/down slide), and releasing keys holds position.", "녹색이면 안정 비행이 활성화됩니다. Space 또는 점프로 상승, Ctrl로 하강, W/A/S/D로 주행·회전하며, 방향키로 차량 위치를 미세 조정합니다(좌/우 옆이동, 위/아래 앞뒤 이동). 키를 놓으면 위치가 유지됩니다.",
                "Turn on to load them now (if attached) and again on each attach. Off skips it for a faster, safer startup.", "켜면 지금(연결된 경우) 불러오고 연결할 때마다 다시 불러옵니다. 끄면 더 빠르고 안전한 시작을 위해 건너뜁니다.",
            });
        }
    }
}
