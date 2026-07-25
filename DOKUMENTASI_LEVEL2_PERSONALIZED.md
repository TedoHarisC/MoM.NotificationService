# Sistem Notifikasi MoM Level 2

## Apa Itu Notifikasi MoM Level 2?

Notifikasi MoM Level 2 adalah pengingat otomatis yang dikirim melalui **email** kepada setiap orang yang terlibat dalam Minutes of Meeting (MoM). Tujuannya sederhana: memastikan setiap item MoM yang belum selesai tidak terlupakan.

Yang membuat sistem ini berbeda dari pengingat biasa adalah — **email dikirim secara personal**. Artinya, setiap orang hanya menerima daftar MoM yang memang menjadi tanggung jawabnya. Tidak ada informasi yang tidak relevan, tidak ada email massal yang mengganggu.

---

## Jenis MoM dan Kapan Diingatkan

Setiap MoM memiliki jenis forum yang menentukan seberapa sering pengingat dikirim.

| Jenis Forum        | Kapan Diingatkan                                       |
| ------------------ | ------------------------------------------------------ |
| **Daily Meeting**  | Setiap 3 hari sekali, dihitung dari tanggal MoM dibuat |
| **Weekly Meeting** | Setelah 5 hari dari tanggal MoM dibuat                 |
| **Draft**          | Tidak ada pengingat                                    |

> **Contoh:** MoM dari Daily Meeting dibuat hari Senin → pengingat dikirim hari Kamis (hari ke-3), lalu Minggu (hari ke-6), lalu Rabu (hari ke-9), dan seterusnya hingga MoM tersebut ditutup.

Semua pengingat dikirim setiap hari pada **pukul 06:30 pagi**.

---

## Siapa yang Menerima Email?

### Karyawan (PIC)

Karyawan yang ditugaskan langsung pada suatu MoM akan menerima email berisi **hanya MoM yang menjadi tugasnya**.

### Dept Head

Dept Head menerima email berisi **semua MoM dari departemennya**. Jika Dept Head juga dilibatkan dalam MoM dari departemen lain, MoM tersebut juga akan muncul di emailnya dengan keterangan khusus.

---

## Contoh Nyata

Bayangkan ada 3 MoM yang sedang berjalan:

|             | MoM 1                 | MoM 2                | MoM 3             |
| ----------- | --------------------- | -------------------- | ----------------- |
| **Topik**   | Pemindahan Alat Berat | Review Prosedur K3   | Kalibrasi Mesin   |
| **Jenis**   | Daily                 | Weekly               | Daily             |
| **Anggota** | Arivian, Rafiq, Muri  | Muri, Angel, Arivian | Rafiq, Muri, Rudy |

Anggap hari ini MoM 1 dan MoM 3 masuk jadwal pengingat Daily, dan MoM 2 masuk jadwal Weekly.

**Email yang dikirim hari ini:**

| Penerima    | MoM yang Diterima   | Keterangan                     |
| ----------- | ------------------- | ------------------------------ |
| **Rafiq**   | MoM 1, MoM 3        | Rafiq ada di MoM 1 dan MoM 3   |
| **Angel**   | MoM 2               | Angel hanya ada di MoM 2       |
| **Arivian** | MoM 1, MoM 2        | Arivian ada di MoM 1 dan MoM 2 |
| **Muri**    | MoM 1, MoM 2, MoM 3 | Muri ada di semua MoM          |
| **Rudy**    | MoM 3               | Rudy hanya ada di MoM 3        |

> **Catatan:** Angel tidak menerima MoM 1 dan MoM 3 karena ia tidak terlibat di sana. Begitu pula Rudy tidak menerima MoM 1 dan MoM 2.

---

## Tampilan Email

Email yang diterima setiap orang berisi:

- **Salam personal** — misalnya _"Yth. Rafiq Andriansyah"_
- **Ringkasan angka** — berapa item yang belum selesai, berapa yang terlambat
- **Tabel daftar MoM** — berisi topik, tindakan yang perlu dilakukan, progress terakhir, batas waktu, dan status
- **Panduan singkat** — cara mengupdate progress di aplikasi Sisfo

Baris yang berwarna **merah muda** menandakan MoM sudah melewati batas waktu.

---

## Email Dept Head

Dept Head menerima email yang sedikit berbeda. Selain MoM dari departemennya sendiri, jika ada MoM dari departemen lain yang melibatkan departemennya, MoM tersebut juga akan muncul dengan tanda khusus:

> ⚑ **Additional PIC — PIC Utama: [Nama Departemen]**

Ini berarti departemen tersebut ikut bertanggung jawab atas MoM tersebut, meskipun bukan pemilik utamanya.

---

## Jaminan Tidak Spam

Sistem dirancang agar tidak mengirim email berlebihan:

- Setiap orang hanya menerima **satu email per MoM per hari** — tidak akan ada duplikat meskipun sistem berjalan berulang
- Email hanya dikirim di hari yang sesuai jadwal (hari ke-3, ke-6, dst untuk Daily — atau setelah 5 hari untuk Weekly)
- MoM yang sudah **Closed** tidak akan dikirimkan lagi

---

## Pertanyaan Umum

**Apakah saya akan menerima email setiap hari?**
Tidak. Anda hanya menerima email pada hari di mana MoM Anda masuk jadwal pengingat. Jika tidak ada MoM yang jadwalnya jatuh hari ini, tidak ada email yang dikirim.

**Bagaimana cara menghentikan pengingat untuk suatu MoM?**
Cukup ubah status MoM menjadi **Closed** di aplikasi Vortex / My Secretary. Pengingat akan berhenti otomatis.

**Apakah saya bisa membalas email ini?**
Email ini dikirim secara otomatis dan tidak dapat dibalas. Untuk mengupdate progress, silakan login ke aplikasi Sisfo (Vortex).
