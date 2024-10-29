# TaskProject

## Proje Açıklaması
TaskProject, kullanıcıların ve yöneticilerin hesap yönetimi, transfer işlemleri ve kullanıcı kaydı gibi işlemleri gerçekleştirebileceği bir web uygulamasıdır. Uygulama, kullanıcı dostu bir arayüz ile birlikte güvenli bir oturum açma ve kayıt sistemi sunmaktadır.

## Kullanılan Teknolojiler
- **ASP.NET MVC**: Uygulamanın temel yapısını oluşturan MVC (Model-View-Controller) mimarisi.
- **Entity Framework**: Veritabanı işlemleri için kullanılan ORM (Object-Relational Mapping) aracı.
- **Bootstrap**: Responsive ve modern bir kullanıcı arayüzü oluşturmak için kullanılan CSS framework'ü.
- **jQuery**: Dinamik içerik güncellemeleri ve AJAX istekleri için kullanılan JavaScript kütüphanesi.
- **BCrypt**: Şifrelerin güvenli bir şekilde hash'lenmesi için kullanılan bir algoritma.

## Kurulum
1. **Proje İndirme**: Projeyi GitHub veya başka bir kaynak üzerinden indirin.

2. **Gerekli Paketlerin Yüklenmesi**: Proje dizinine gidin ve NuGet paketlerini yükleyin.

3. **Veritabanı Ayarları**: `appsettings.json` dosyasını açın ve veritabanı bağlantı dizesini güncelleyin.

4. **Veritabanı Bağlantısını Yapın**

5. **Uygulamayı Başlatabilirsiniz**

## Kullanım
- **Kullanıcı Girişi**: Uygulama ana sayfasında "Giriş Yap" butonuna tıklayarak kullanıcı girişi yapabilirsiniz. (user@gmail.com ve 123456 veya user1@gmail.com  123456 bilgileriyle giriş yapabilisiniz )
- **Kayıt Olma**: Eğer bir hesabınız yoksa, "Buradan kaydolun" bağlantısına tıklayarak yeni bir hesap oluşturabilirsiniz.
- **Admin Girişi**: Admin kullanıcıları için sağ üst köşede "Admin Girişi" bağlantısı bulunmaktadır. Bu bağlantı ile admin paneline erişebilirsiniz.   (admin@gmail.com ve 123456)

## Özellikler
- Kullanıcı ve admin hesap yönetimi.
- Transfer işlemleri (gelen - giden - iptal).
- Kullanıcı kayıt ve giriş işlemleri.
- Şifre güncelleme ve telefon numarası güncelleme.


