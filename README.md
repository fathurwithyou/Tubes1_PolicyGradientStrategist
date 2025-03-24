# Greedy Algorithm Bot for Robocode Tank Royale

## Deskripsi

**Robocode Tank Royale** adalah permainan pemrograman di mana pemain merancang bot berbentuk tank virtual yang bertarung dalam arena hingga hanya tersisa satu pemenang. Pemain mengembangkan algoritma bot yang mengatur strategi pergerakan, deteksi lawan, dan serangan tanpa kendali langsung selama pertempuran. Bot ini diimplementasikan menggunakan **C#** dengan strategi **greedy**.

### Algoritma Greedy yang Diimplementasikan

1. **Ellipsis**: Bot ini memaksimalkan **survival rate** dan **bullet damage** dengan menghindari pertempuran jarak dekat. Menggunakan gerakan mengorbit dan strategi defensif untuk bertahan lebih lama sambil memberikan tembakan yang akurat.
2. **Hebi**: Bot ini berfokus pada **ramming damage** dan **bullet damage**. Menggunakan pendekatan agresif dengan mendekati lawan dan menabraknya untuk mendapatkan skor tambahan, sambil menembak dari jarak dekat.
3. **Kawakaze**: Menerapkan strategi **minimum risk movement** untuk meminimalkan risiko terkena tembakan sambil tetap menyerang musuh. Berusaha menjaga jarak yang aman untuk meningkatkan survival rate dan memberikan damage maksimal.
4. **LowestEnergyChaser**: Bot ini mengincar musuh dengan **energi terendah** untuk memaksimalkan **killing score**. Menggunakan pendekatan agresif untuk memastikan musuh dengan HP rendah dieliminasi dengan cepat, baik melalui tembakan maupun tabrakan.

## Requirement

Sebelum menjalankan program, pastikan Anda memiliki beberapa dependensi berikut:

- **.NET 8.0 atau lebih baru** (untuk menjalankan aplikasi C#).
- **Robocode Tank Royale**: Program permainan.

## Cara menjalankan program

1. Clone repository ini ke mesin lokal Anda:

   ```bash
   git clone https://github.com/fathurwithyou/Tubes1_PolicyGradientStrategist
   ```

2. Dalam setiap folder bot yang ingin digunakan, edit file NamaBot.csproj pada bagian TargetFramework dengan mengubahnya menjadi versi .NET anda:

3. Hapus folder bin dan obj, dan jalankan:

### Bash

```bash
./NamaBot.sh
```

### Command Prompt

```cmd
./NamaBot.cmd
```

4. Jalankan aplikasi Robocode, konfigurasi Robocode untuk melihat folder bot sesuai tempat instalasi anda, dan masukkan bot yang sudah dibangun ke dalam arena.

## Author

| Nama                     | NIM      | Email                       |
| ------------------------ | -------- | --------------------------- |
| William Andrian Dharma T | 13523006 | 13523006@std.stei.itb.ac.id |
| Muhammad Dicky Isra      | 13523075 | 13523075@std.stei.itb.ac.id |
| Muhammad Fathur Rizky    | 13523105 | 13523105@std.stei.itb.ac.id |
