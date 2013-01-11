MaestroPanel Import Tool 1.0
======

Bu araç MaestroPanel'e göç aracıdır. MaestroPanel'e aktarmak istediğiniz yazılımı bulunduğu yerden alır ve MaestroPanel üzerinde çalışacak şekilde yapılandırır.

**Desteklenen Yazılımlar:**

* Plesk 8.5 ve üzeri (Ms Access, MySQL, MsSQL databaseleri).
* Entrenix 2.5 ve üzeri.

## Kurulum (Install)

mpimport.exe kendi başına çalışacak şekilde yapılandırılmıştır. Aktarmak istedğiniz sunucuya kopyalamanız yeterli.

## Başlagıç

Sıkıştırılmış dosyayı açtıktan sonra yapılması ilk gereken hareket mpimport.exe.config dosyasının ayarlanması olmalıdır. 
Bu dosya içinde aktarım için gerekli olan parametreler mevcuttur. mpimport.exe herhangi bir parametre almaz. Tüm parametreler .config dosyasının içinde tanımlanmalıdır.


## Config

**dbtype**

mpimport'un hangi veritabanına bağlanacağını belirler buraya girebileceğiniz alanlar aşağıdaki gibidir.

* **mssql** - Plesk Microsoft SQL Server kullanıyorsa girilecek değer
* **mysql** - Plesk MySQL kullanıyorsa girilecek değer
* **access** - Plesk Microfost Access Database kullanıyorsa girilecek değer
* **mpsqlite** - MaestroPanel SQLite kullanıyorsa girilecek değer.

**connectionString**

Kaynak veritabanına bağlanılacak connection string cümlesi.

**apiKey**

Hedef MaestroPanel'de üretilen API Anahtarı. Bu Anahtar hangi kullanıcıya tanımlıysa domainler onun altında açılır. API ile ilgili daha fazla bilgi için http://wiki.maestropanel.com/MaestroPanel-API.ashx

**host**

Hedef MaestroPanel'in IP adresi veya host ismi.

**port**

Hedef MaestroPanel'in çalıştığı port. Varsayılan olarak 9715 dir.

**domainPlanName**

Hedef MaestroPanel'de daha önce tanımlanmış domain planı adı. Buraya takma ismi (alias) girmeniz gerekiyor. Daha fazla bilgi için http://wiki.maestropanel.com/Domain-Planı-Olusturmak.ashx

**resellerName**

Plesk'ten MaestroPanel aktarımlarında dikkate alınır. Plesk üzerindeki tek bir reseller'ı aktaracaksanız onun login isminin yazılması gerekiyor.

**logging**

Loglamanın yapılıp, yapılmamasını belirler. True veya False değeri alabilir.

**importEmails**

Email'lerin taşınıp taşınmayacağını belirler. True veya False değeri alabilir.

**importDomains**

Domainlerin taşınıp taşınmayacağını belirler. True veya False değeri alabilir.

**importDatabases**

Veritabanlarının taşınıp taşınmayacağını belirler. Hem MySQL hemde MSSQL'leri aynı anda taşır. True veya False değeri alabilir.

**importSubdomains**

Subdomainlerin taşınıp taşınmayacağını belirler. True veya False değeri alabilir.

**importDomainAlias**

Domain aliasların taşınıp taşınmayacağını belirler. True veya False değeri alabilir.

CopyFiles Web sitesindeki dosyaların taşınıp taşınmayacağını belirler. True veya False değeri alabilir.

**SourceDirPattern**

Eğer CopyFiles True ise dikkate alınır. Kopyalanacak kaynak dizinin desenini belirler {DOMAIN} değişkendir.

**DestinationDirPattern**

Eğer CopyFiles True ise dikkate alınır. Kopyalanacak hedef dizinin desenini belirler. {DESTINATION} ve {DOMAIN} değişkendir. {DESTINATION} değişkeni DestinationServerIp değerine ne girildiyse o olur.

**DestinationServerIp**

Hedef sunucunun IP adresi.

**DestinationServerPassword**

Hedef sunucunun parolası.

**DestinationServerUsername**

Hedef sunucunun kullanıcı adı.

**SourceDirEmailPattern**

Eposta kutularının içeriğini taşımak için kullanılır. Kaynak deseni belirler. CopyEmailFiles True ise dikkate alınır.

**DestinationDirEmailPatter**

Eposta kutularının içeriğini taşımak için kullanılır. Hedef deseni belirler. CopyEmailFiles True ise dikkate alınır.

**CopyEmailFiles**

Eposta kutularının içeriğinin taşınıp taşınmamasını belirler. True veya False değer alabilir.
