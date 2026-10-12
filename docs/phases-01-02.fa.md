# مرحله‌های ۱ و ۲: اسکلت پروژه و Domain

این نسخه شامل کد کامل این دو مرحله است. SQL Server، JWT، Swagger، RabbitMQ و Docker در مرحله‌های بعدی اضافه می‌شوند.

## مرحلهٔ ۱: وابستگی‌های پروژه

| پروژه | مسئولیت | وابستگی |
| --- | --- | --- |
| Domain | مدل‌ها و قوانین کسب‌وکار | هیچ پروژه یا پکیج خارجی |
| Application | اجرای سناریوها و تعریف قراردادها | Domain |
| Infrastructure | پیاده‌سازی ذخیره‌سازی و ارتباط‌ها | Application و Domain |
| API | ورودی HTTP و ثبت وابستگی‌ها | Application و Infrastructure |
| Worker | اجرای Consumerها | Application و Infrastructure |

Domain نباید به EF Core، ASP.NET یا RabbitMQ وابسته باشد. Reference از API به Infrastructure برای ثبت وابستگی‌ها در نقطهٔ شروع برنامه است.

### اجرا در سیستم شما

پس از استخراج فایل، در پوشهٔ `OrderManagement` اجرا کنید:

```powershell
dotnet --version
dotnet restore OrderManagement.slnx
dotnet build OrderManagement.slnx --no-restore
dotnet run --project src/OrderManagement.Api
```

با مرورگر یا ابزار HTTP آدرس زیر را باز کنید:

```text
http://localhost:5080/health/live
```

پاسخ مورد انتظار `Healthy` است. Swagger را در مرحلهٔ API اضافه می‌کنیم. این مسیر صرفاً زنده‌بودن پردازش را نشان می‌دهد.

Worker هم از یک ترمینال جدا اجرا می‌شود:

```powershell
dotnet run --project src/OrderManagement.Worker
```

در حال حاضر Host بالا می‌آید و منتظر خاموش‌شدن می‌ماند. Consumer هنوز ثبت نشده است.

### فایل‌های تنظیمات

- `Directory.Build.props`: تنظیمات مشترک همهٔ پروژه‌ها؛ `net10.0`، Nullable، ImplicitUsings و خطاگرفتن از Warningها.
- `Directory.Packages.props`: نگهداری نسخهٔ پکیج‌ها در یک نقطه. فعلاً فقط Hosting برای Worker لازم است.
- `global.json`: انتخاب SDK پایدار دات‌نت ۱۰؛ اجازهٔ استفاده از Feature Band جدیدتر همین نسخه داده شده است.
- `OrderManagement.slnx`: فایل Solution با فرمت XML.

کامیت این مرحله:

```text
chore: initialize .NET 10 clean architecture solution
```

## مرحلهٔ ۲: مدل و قوانین Domain

| مدل | مسئولیت |
| --- | --- |
| Customer | نام و ایمیل مشتری |
| Product | SKU، نام و قیمت فعلی محصول |
| InventoryStock | تعداد موجودی هر محصول و جلوگیری از مقدار منفی |
| Order | مدیریت وضعیت، آیتم‌ها، مبلغ و حذف نرم سفارش |
| OrderItem | تعداد و قیمت ثبت‌شدهٔ محصول در همان سفارش |
| OrderStatusHistory | وضعیت قبل/بعد، عامل تغییر، زمان و Revision |

### چرا Order یک Aggregate Root است؟

Order مسئول حفظ سازگاری آیتم‌ها و وضعیت خودش است. بیرون از Domain نمی‌توانیم مستقیماً بنویسیم:

```csharp
// These do not compile: setters are private.
order.Status = OrderStatus.Delivered;
order.TotalAmount = 1;
```

ساخت سفارش از Factory انجام می‌شود:

```csharp
using OrderManagement.Domain.Orders;

DateTimeOffset now = DateTimeOffset.UtcNow;

var order = Order.Create(
    customerId: 1,
    items:
    [
        new OrderItemData(ProductId: 10, Quantity: 2, UnitPrice: 150m),
        new OrderItemData(ProductId: 20, Quantity: 1, UnitPrice: 80m)
    ],
    now: now,
    createdBy: "user-1");

// Pending, total = 380, revision = 1.
```

این `UnitPrice` باید در Application از محصول موجود در دیتابیس خوانده شود. قرارداد HTTP آینده فقط ProductId و Quantity دریافت می‌کند. وجود Customer و Product نیز در Application بررسی و با Foreign Key در SQL Server تضمین خواهد شد؛ مثبت‌بودن شناسه به‌تنهایی اثبات وجود رکورد نیست.

قیمت OrderItem یک Snapshot است. تغییر قیمت فعلی Product، مبلغ سفارش گذشته را عوض نمی‌کند.

### قوانین ایجاد سفارش

- حداقل یک محصول و حداکثر ۱۰۰ محصول متمایز.
- شناسه‌ها و تعداد باید مثبت باشند.
- محصول تکراری رد می‌شود؛ در صورت نیاز Application تعدادها را قبل از ساخت سفارش تجمیع می‌کند.
- قیمت مثبت با حداکثر دو رقم اعشار؛ مدل اولیه محصول رایگان ندارد.
- مبلغ آیتم و جمع سفارش باید در محدودهٔ `decimal(18,2)` باشند.
- آیتم‌ها از روی ورودی ساخته می‌شوند و Collection خروجی فقط خواندنی است.

### تغییر وضعیت

```csharp
order.Confirm(now.AddMinutes(1), "admin-1");
order.Ship(now.AddMinutes(2), "admin-1");
order.Deliver(now.AddMinutes(3), "admin-1");
```

تنها مسیر مجاز:

```text
Pending -> Confirmed -> Shipped -> Delivered
```

برای نمونه، Ship روی سفارش Pending یا Confirm دوباره روی سفارش Confirmed، `DomainException` با کد `order.invalid_status_transition` ایجاد می‌کند و تغییری در سفارش نمی‌دهد.

هر تغییر موفق یک رکورد تاریخچه می‌سازد و Revision را افزایش می‌دهد. زمان و شناسهٔ عامل از Application وارد می‌شوند؛ Domain به ساعت سیستم یا HttpContext دسترسی ندارد. زمان‌ها به UTC تبدیل می‌شوند.

### موجودی و تراکنش: تفاوتی مهم

```csharp
using OrderManagement.Domain.Inventory;

var stock = InventoryStock.Create(productId: 10, availableQuantity: 3);
stock.Decrease(2); // AvailableQuantity = 1.
stock.Decrease(2); // Throws; AvailableQuantity remains 1.
```

این کنترل، وضعیت همان شیء در حافظه را حفظ می‌کند. دو درخواست مستقل ممکن است هر دو موجودی قبلی را خوانده باشند. بنابراین جلوگیری از فروش بیش از موجودی در مرحلهٔ تأیید، با UPDATE شرطی و تراکنش SQL پیاده می‌شود.

`Order.Confirm` خودش به دیتابیس وصل نمی‌شود. Handler تأیید مسئول کنترل/کاهش اتمیک موجودی و ثبت سفارش، تاریخچه و Outbox در یک تراکنش خواهد بود. تا اجرای آن مرحله، نباید این نسخه را دارای کنترل موجودی هم‌زمان بدانیم.

`Revision` ترتیب منطقی تغییرهاست؛ جایگزین `rowversion` SQL Server نیست. RowVersion در تنظیمات Infrastructure اضافه می‌شود.

### حذف نرم

```csharp
order.Delete(now.AddMinutes(4), "admin-1");
```

عامل و زمان حذف ثبت می‌شود و سفارش حذف‌شده دیگر تغییر وضعیت نمی‌دهد. حذف معادل لغو سفارش یا برگشت موجودی نیست. تشخیص Admin بودن عامل در لایهٔ دسترسی انجام می‌شود؛ ارسال رشتهٔ `admin-1` به این متد به‌تنهایی مجوز ایجاد نمی‌کند.

کامیت مرحلهٔ دوم:

```text
feat: implement order domain and business rules
```

## تاریخچهٔ Git همراه فایل

آرشیو علاوه بر کد، فایل `OrderManagement-history.bundle` دارد. برای دریافت مخزن با همان دو کامیت، از پوشه‌ای که Bundle در آن قرار دارد اجرا کنید:

```powershell
git clone .\OrderManagement-history.bundle OrderManagement-with-history
cd OrderManagement-with-history
git log --oneline
```

Bundle فقط تاریخچهٔ محلی این نسخه است؛ هیچ چیزی به حساب GitHub شما Push نشده است.

## گام بعدی

مرحلهٔ ۳ شامل OrderDbContext، Fluent Configuration برای Collectionهای خصوصی، RowVersion، ایندکس‌ها، Migration و Seed مشتری‌ها/محصول‌ها است. Read و Write Repository با قراردادهای Application و یک DbContext مشترک اضافه خواهند شد. پروژه‌های تست رسمی طبق برنامه در مرحلهٔ تست ساخته می‌شوند.
