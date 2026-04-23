using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Models.Database;

public partial class ArchiveFqpContext : DbContext
{
    public ArchiveFqpContext()
    {
    }

    public ArchiveFqpContext(DbContextOptions<ArchiveFqpContext> options)
        : base(options)
    {
    }

    public virtual DbSet<АккаунтПользователя> АккаунтПользователяs { get; set; }

    public virtual DbSet<Атрибут> Атрибутs { get; set; }

    public virtual DbSet<АтрибутИнститута> АтрибутИнститутаs { get; set; }

    public virtual DbSet<АтрибутКафедры> АтрибутКафедрыs { get; set; }

    public virtual DbSet<АтрибутНаправления> АтрибутНаправленияs { get; set; }

    public virtual DbSet<АтрибутПрофиля> АтрибутПрофиляs { get; set; }

    public virtual DbSet<АтрибутУчреждения> АтрибутУчрежденияs { get; set; }

    public virtual DbSet<ДанныеПоАтриб> ДанныеПоАтрибs { get; set; }

    public virtual DbSet<Должность> Должностьs { get; set; }

    public virtual DbSet<ДоступРаботы> ДоступРаботыs { get; set; }

    public virtual DbSet<ЖурналДействий> ЖурналДействийs { get; set; }
    
    public virtual DbSet<ЗаявлениеРаботы> ЗаявлениеРаботыs { get; set; }

    public virtual DbSet<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; }

    public virtual DbSet<Институт> Институтs { get; set; }

    public virtual DbSet<Кафедра> Кафедраs { get; set; }

    public virtual DbSet<Консультант> Консультантs { get; set; }

    public virtual DbSet<Направление> Направлениеs { get; set; }

    public virtual DbSet<ОценкаПреподавателя> ОценкаПреподавателяs { get; set; }

    public virtual DbSet<Пользователь> Пользовательs { get; set; }

    public virtual DbSet<Преподаватель> Преподавательs { get; set; }

    public virtual DbSet<Профиль> Профильs { get; set; }

    public virtual DbSet<Работа> Работаs { get; set; }

    public virtual DbSet<Рецензент> Рецензентs { get; set; }

    public virtual DbSet<РольПользователя> РольПользователяs { get; set; }

    public virtual DbSet<СтатусРаботы> СтатусРаботыs { get; set; }

    public virtual DbSet<СтатусЗаявления> СтатусЗаявленияs { get; set; }

    public virtual DbSet<Студент> Студентs { get; set; }

    public virtual DbSet<ТипРаботы> ТипРаботыs { get; set; }

    public virtual DbSet<Угсн> Угснs { get; set; }

    public virtual DbSet<УгснСтандарт> УгснСтандартs { get; set; }

    public virtual DbSet<УровеньОбразования> УровеньОбразованияs { get; set; }

    public virtual DbSet<ФормаОбучения> ФормаОбученияs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<АккаунтПользователя>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("аккаунт_пользователя");

            entity.HasIndex(e => e.IdПользователя, "аккаунт_пользова_id_пользователя_key").IsUnique();

            entity.Property(e => e.IdПользователя).HasColumnName("id_пользователя");
            entity.Property(e => e.Логин)
                .HasMaxLength(60)
                .HasColumnName("логин");
            entity.Property(e => e.Пароль)
                .HasColumnType("character varying")
                .HasColumnName("пароль");
            entity.Property(e => e.Роли).HasColumnName("роли");

            entity.HasOne(d => d.IdПользователяNavigation).WithOne()
                .HasForeignKey<АккаунтПользователя>(d => d.IdПользователя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("аккаунт_пользов_id_пользователя_fkey");
        });

        modelBuilder.Entity<Атрибут>(entity =>
        {
            entity.HasKey(e => e.IdАтрибута).HasName("атрибут_pkey");

            entity.ToTable("атрибут");

            entity.HasIndex(e => e.Название, "атрибут_название_key").IsUnique();

            entity.Property(e => e.IdАтрибута).HasColumnName("id_атрибута");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<АтрибутИнститута>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("атрибут_института");

            entity.HasIndex(e => new { e.IdИнститута, e.IdАтрибута, e.IdСтатусаРаботы, e.IdТипаРаботы }, "атрибут_института_атрибут_key").IsUnique();

            entity.Property(e => e.IdАтрибута).HasColumnName("id_атрибута");
            entity.Property(e => e.IdИнститута).HasColumnName("id_института");
            entity.Property(e => e.IdСтатусаРаботы).HasColumnName("id_статуса_работы");
            entity.Property(e => e.IdСтруктуры)
                .HasDefaultValueSql("nextval('\"атрибут_учреждения_id_структуры_seq\"'::regclass)")
                .HasColumnName("id_структуры");
            entity.Property(e => e.IdТипаРаботы).HasColumnName("id_типа_работы");

            entity.HasOne(d => d.IdИнститутаNavigation).WithMany()
                .HasForeignKey(d => d.IdИнститута)
                .HasConstraintName("атрибут_института_id_института_fkey");
        });

        modelBuilder.Entity<АтрибутКафедры>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("атрибут_кафедры");

            entity.HasIndex(e => new { e.IdКафедры, e.IdАтрибута, e.IdСтатусаРаботы, e.IdТипаРаботы }, "атрибут_кафедры_атрибут_key").IsUnique();

            entity.Property(e => e.IdАтрибута).HasColumnName("id_атрибута");
            entity.Property(e => e.IdКафедры).HasColumnName("id_кафедры");
            entity.Property(e => e.IdСтатусаРаботы).HasColumnName("id_статуса_работы");
            entity.Property(e => e.IdСтруктуры)
                .HasDefaultValueSql("nextval('\"атрибут_учреждения_id_структуры_seq\"'::regclass)")
                .HasColumnName("id_структуры");
            entity.Property(e => e.IdТипаРаботы).HasColumnName("id_типа_работы");

            entity.HasOne(d => d.IdКафедрыNavigation).WithMany()
                .HasForeignKey(d => d.IdКафедры)
                .HasConstraintName("атрибут_кафедры_id_кафедры_fkey");
        });

        modelBuilder.Entity<АтрибутНаправления>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("атрибут_направления");

            entity.HasIndex(e => new { e.IdНаправления, e.IdАтрибута, e.IdСтатусаРаботы, e.IdТипаРаботы }, "атрибут_направления_атрибут_key").IsUnique();

            entity.Property(e => e.IdАтрибута).HasColumnName("id_атрибута");
            entity.Property(e => e.IdНаправления).HasColumnName("id_направления");
            entity.Property(e => e.IdСтатусаРаботы).HasColumnName("id_статуса_работы");
            entity.Property(e => e.IdСтруктуры)
                .HasDefaultValueSql("nextval('\"атрибут_учреждения_id_структуры_seq\"'::regclass)")
                .HasColumnName("id_структуры");
            entity.Property(e => e.IdТипаРаботы).HasColumnName("id_типа_работы");

            entity.HasOne(d => d.IdНаправленияNavigation).WithMany()
                .HasForeignKey(d => d.IdНаправления)
                .HasConstraintName("атрибут_направле_id_направления_fkey");
        });

        modelBuilder.Entity<АтрибутПрофиля>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("атрибут_профиля");

            entity.HasIndex(e => new { e.IdПрофиля, e.IdАтрибута, e.IdСтатусаРаботы, e.IdТипаРаботы }, "атрибут_профиля_атрибут_key").IsUnique();

            entity.Property(e => e.IdАтрибута).HasColumnName("id_атрибута");
            entity.Property(e => e.IdПрофиля).HasColumnName("id_профиля");
            entity.Property(e => e.IdСтатусаРаботы).HasColumnName("id_статуса_работы");
            entity.Property(e => e.IdСтруктуры)
                .HasDefaultValueSql("nextval('\"атрибут_учреждения_id_структуры_seq\"'::regclass)")
                .HasColumnName("id_структуры");
            entity.Property(e => e.IdТипаРаботы).HasColumnName("id_типа_работы");

            entity.HasOne(d => d.IdПрофиляNavigation).WithMany()
                .HasForeignKey(d => d.IdПрофиля)
                .HasConstraintName("атрибут_профиля_id_профиля_fkey");
        });

        modelBuilder.Entity<АтрибутУчреждения>(entity =>
        {
            entity.HasKey(e => e.IdСтруктуры).HasName("атрибут_учреждения_pkey");

            entity.ToTable("атрибут_учреждения");

            entity.HasIndex(e => new { e.IdАтрибута, e.IdСтатусаРаботы, e.IdТипаРаботы }, "атрибут_учреждения_атрибут_key").IsUnique();

            entity.Property(e => e.IdСтруктуры).HasColumnName("id_структуры");
            entity.Property(e => e.IdАтрибута).HasColumnName("id_атрибута");
            entity.Property(e => e.IdСтатусаРаботы).HasColumnName("id_статуса_работы");
            entity.Property(e => e.IdТипаРаботы).HasColumnName("id_типа_работы");

            entity.HasOne(d => d.IdАтрибутаNavigation).WithMany(p => p.АтрибутУчрежденияs)
                .HasForeignKey(d => d.IdАтрибута)
                .HasConstraintName("атрибут_учреждения_id_атрибута_fkey");

            entity.HasOne(d => d.IdСтатусаРаботыNavigation).WithMany(p => p.АтрибутУчрежденияs)
                .HasForeignKey(d => d.IdСтатусаРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("атрибут_учрежде_id_статуса_работ_fkey");

            entity.HasOne(d => d.IdТипаРаботыNavigation).WithMany(p => p.АтрибутУчрежденияs)
                .HasForeignKey(d => d.IdТипаРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("атрибут_учреждени_id_типа_работы_fkey");
        });

        modelBuilder.Entity<ДанныеПоАтриб>(entity =>
        {
            entity.HasKey(e => e.IdДанных).HasName("данные_по_атриб_pkey");

            entity.ToTable("данные_по_атриб");

            entity.HasIndex(e => new { e.IdРаботы, e.IdСтруктуры }, "данные_по_атриб_атрибут_key").IsUnique();

            entity.Property(e => e.IdДанных).HasColumnName("id_данных");
            entity.Property(e => e.IdРаботы).HasColumnName("id_работы");
            entity.Property(e => e.IdСтруктуры).HasColumnName("id_структуры");
            entity.Property(e => e.Данные)
                .HasDefaultValueSql("'Ожидание поиска...'::text")
                .HasColumnName("данные");

            entity.HasOne(d => d.IdРаботыNavigation).WithMany()
                .HasForeignKey(d => d.IdРаботы)
                .HasConstraintName("данные_по_атриб_id_работы_fkey");
        });

        modelBuilder.Entity<Должность>(entity =>
        {
            entity.HasKey(e => e.IdДолжности).HasName("должность_pkey");

            entity.ToTable("должность");

            entity.HasIndex(e => e.Название, "должность_название_key").IsUnique();

            entity.Property(e => e.IdДолжности).HasColumnName("id_должности");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<ДоступРаботы>(entity =>
        {
            entity.HasKey(e => e.IdДоступаРаботы).HasName("доступ_работы_pkey");

            entity.ToTable("доступ_работы");

            entity.HasIndex(e => e.Название, "доступ_работы_название_key").IsUnique();

            entity.Property(e => e.IdДоступаРаботы).HasColumnName("id_доступа_работы");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<ЖурналДействий>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("журнал_действий");

            entity.Property(e => e.Время)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("время");
            entity.Property(e => e.Запрос).HasColumnName("запрос");
            entity.Property(e => e.НовыеДанные)
                .HasColumnType("character varying")
                .HasColumnName("новые_данные");
            entity.Property(e => e.Операция)
                .HasColumnType("character varying")
                .HasColumnName("операция");
            entity.Property(e => e.СтарыеДанные)
                .HasColumnType("character varying")
                .HasColumnName("старые_данные");
            entity.Property(e => e.Схема)
                .HasColumnType("character varying")
                .HasColumnName("схема");
            entity.Property(e => e.Таблица)
                .HasColumnType("character varying")
                .HasColumnName("таблица");
        });

        modelBuilder.Entity<ЗаявлениеАтрибута>(entity =>
        {
            entity.HasKey(e => e.IdЗаявления).HasName("заявление_атрибута_pkey");

            entity.ToTable("заявление_атрибута");

            entity.Property(e => e.IdЗаявления).HasColumnName("id_заявления");
            entity.Property(e => e.IdПользователя).HasColumnName("id_пользователя");
            entity.Property(e => e.IdАтрибута).HasColumnName("id_атрибута");
            entity.Property(e => e.IdИнститута).HasColumnName("id_института");
            entity.Property(e => e.IdКафедры).HasColumnName("id_кафедры");
            entity.Property(e => e.IdНаправления).HasColumnName("id_направления");
            entity.Property(e => e.IdПрофиля).HasColumnName("id_профиля");
            entity.Property(e => e.IdСтатуса).HasColumnName("id_статуса");
            entity.Property(e => e.ДатаОтвета)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_ответа");
            entity.Property(e => e.ДатаПоступления)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_поступления");
            entity.Property(e => e.Ответ)
                .HasColumnType("character varying")
                .HasColumnName("ответ");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
            entity.Property(e => e.Описание)
                .HasColumnType("character varying")
                .HasColumnName("описание");
            entity.Property(e => e.Новый)
                .HasColumnType("boolean")
                .HasColumnName("новый");
            entity.Property(e => e.Примеры).HasColumnName("примеры");

            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.ЗаявлениеАтрибутаs)
                .HasForeignKey(d => d.IdПользователя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("заявление_атрибута_id_пользователя_fkey");

            entity.HasOne(d => d.IdАтрибутаNavigation).WithMany(p => p.ЗаявлениеАтрибутаs)
                .HasForeignKey(d => d.IdАтрибута)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("заявление_атрибута_id_атрибута_fkey");

            entity.HasOne(d => d.IdИнститутаNavigation).WithMany(p => p.ЗаявлениеАтрибутаs)
                .HasForeignKey(d => d.IdИнститута)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("заявление_атрибута_id_института_fkey");

            entity.HasOne(d => d.IdКафедрыNavigation).WithMany(p => p.ЗаявлениеАтрибутаs)
                .HasForeignKey(d => d.IdКафедры)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("заявление_атрибута_id_кафедры_fkey");

            entity.HasOne(d => d.IdНаправленияNavigation).WithMany(p => p.ЗаявлениеАтрибутаs)
                .HasForeignKey(d => d.IdНаправления)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("заявление_атрибута_id_направления_fkey");

            entity.HasOne(d => d.IdПрофиляNavigation).WithMany(p => p.ЗаявлениеАтрибутаs)
                .HasForeignKey(d => d.IdПрофиля)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("заявление_атрибута_id_профиля_fkey");

            entity.HasOne(d => d.IdСтатусЗаявленияNavigation).WithMany(p => p.ЗаявлениеАтрибутаs)
                .HasForeignKey(d => d.IdСтатуса)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("заявление_атрибута_id_статуса_заявления_fkey");
        });

        modelBuilder.Entity<ЗаявлениеРаботы>(entity =>
        {
            entity.HasKey(e => e.IdЗаявления).HasName("заявление_работы_pkey");

            entity.ToTable("заявление_работы");

            entity.Property(e => e.IdЗаявления).HasColumnName("id_заявления");
            entity.Property(e => e.IdПользователя).HasColumnName("id_пользователя");
            entity.Property(e => e.IdСтатуса).HasColumnName("id_статуса");
            entity.Property(e => e.IdРаботы).HasColumnName("id_работы");
            entity.Property(e => e.ДатаВозврПоЗаявл)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_возвр_по_заявл");
            entity.Property(e => e.ДатаВозврПоФакту)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_возвр_по_факту");
            entity.Property(e => e.ДатаОтвета)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_ответа");
            entity.Property(e => e.ДатаПоступления)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_поступления");
            entity.Property(e => e.Ответ)
                .HasColumnType("character varying")
                .HasColumnName("ответ");
            entity.Property(e => e.Цель)
                .HasColumnType("character varying")
                .HasColumnName("цель");

            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.ЗаявлениеРаботыs)
                .HasForeignKey(d => d.IdПользователя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("заявление_работы_id_пользователя_fkey");

            entity.HasOne(d => d.IdРаботыNavigation).WithMany(p => p.ЗаявлениеРаботыs)
                .HasForeignKey(d => d.IdРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("заявление_работы_id_работы_fkey");

            entity.HasOne(d => d.IdСтатусЗаявленияNavigation).WithMany(p => p.ЗаявлениеРаботыs)
                .HasForeignKey(d => d.IdСтатуса)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("заявление_работы_id_статуса_заявления_fkey");
        });

        modelBuilder.Entity<Институт>(entity =>
        {
            entity.HasKey(e => e.IdИнститута).HasName("институт_pkey");

            entity.ToTable("институт");

            entity.HasIndex(e => e.Название, "институт_название_key").IsUnique();

            entity.Property(e => e.IdИнститута).HasColumnName("id_института");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<Кафедра>(entity =>
        {
            entity.HasKey(e => e.IdКафедры).HasName("кафедра_pkey");

            entity.ToTable("кафедра");

            entity.HasIndex(e => new { e.Название, e.IdУгсн }, "кафедра_кафедра_key").IsUnique();

            entity.Property(e => e.IdКафедры).HasColumnName("id_кафедры");
            entity.Property(e => e.IdИнститута).HasColumnName("id_института");
            entity.Property(e => e.IdУгсн).HasColumnName("id_угсн");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");

            entity.HasOne(d => d.IdИнститутаNavigation).WithMany(p => p.Кафедраs)
                .HasForeignKey(d => d.IdИнститута)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("кафедра_id_института_fkey");

            entity.HasOne(d => d.IdУгснNavigation).WithMany(p => p.Кафедраs)
                .HasForeignKey(d => d.IdУгсн)
                .HasConstraintName("кафедра_id_угсн_fkey");
        });

        modelBuilder.Entity<Консультант>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("консультант_pkey");

            entity.ToTable("консультант");

            entity.HasIndex(e => new { e.IdРаботы, e.IdПреподавателя }, "консультант_работа_препод_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdДолжности).HasColumnName("id_должности");
            entity.Property(e => e.IdПреподавателя).HasColumnName("id_преподавателя");
            entity.Property(e => e.IdРаботы).HasColumnName("id_работы");

            entity.HasOne(d => d.IdДолжностиNavigation).WithMany(p => p.Консультантs)
                .HasForeignKey(d => d.IdДолжности)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("консультант_id_должности_fkey");

            entity.HasOne(d => d.IdПреподавателяNavigation).WithMany(p => p.Консультантs)
                .HasForeignKey(d => d.IdПреподавателя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("консультант_id_преподавателя_fkey");

            entity.HasOne(d => d.IdРаботыNavigation).WithMany(p => p.Консультантs)
                .HasForeignKey(d => d.IdРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("консультант_id_работы_fkey");
        });

        modelBuilder.Entity<Направление>(entity =>
        {
            entity.HasKey(e => e.IdНаправления).HasName("направление_pkey");

            entity.ToTable("направление");

            entity.HasIndex(e => new { e.Название, e.IdКафедры }, "направление_направление_key").IsUnique();

            entity.Property(e => e.IdНаправления).HasColumnName("id_направления");
            entity.Property(e => e.IdКафедры).HasColumnName("id_кафедры");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");

            entity.HasOne(d => d.IdКафедрыNavigation).WithMany(p => p.Направлениеs)
                .HasForeignKey(d => d.IdКафедры)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("направление_id_кафедры_fkey");
        });

        modelBuilder.Entity<ОценкаПреподавателя>(entity =>
        {
            entity.HasKey(e => e.IdОценки).HasName("оценка_преподавателя_pkey");

            entity.ToTable("оценка_преподавателя");

            entity.Property(e => e.IdОценки).HasColumnName("id_оценки");
            entity.Property(e => e.IdПреподавателя).HasColumnName("id_преподавателя");
            entity.Property(e => e.IdРаботы).HasColumnName("id_работы");
            entity.Property(e => e.Отзыв).HasColumnName("отзыв");
            entity.Property(e => e.Оценка).HasColumnName("оценка");

            entity.HasOne(d => d.IdПреподавателяNavigation).WithMany(p => p.ОценкаПреподавателяs)
                .HasForeignKey(d => d.IdПреподавателя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("оценка_преподав_id_преподавател_fkey");

            entity.HasOne(d => d.IdРаботыNavigation).WithMany(p => p.ОценкаПреподавателяs)
                .HasForeignKey(d => d.IdРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("оценка_преподавателя_id_работы_fkey");
        });

        modelBuilder.Entity<Пользователь>(entity =>
        {
            entity.HasKey(e => e.IdПользователя).HasName("пользователь_pkey");

            entity.ToTable("пользователь");

            entity.HasIndex(e => e.Email, "пользователь_email_key").IsUnique();

            entity.Property(e => e.IdПользователя).HasColumnName("id_пользователя");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.Имя)
                .HasMaxLength(50)
                .HasColumnName("имя");
            entity.Property(e => e.Отчество)
                .HasMaxLength(50)
                .HasColumnName("отчество");
            entity.Property(e => e.Фамилия)
                .HasMaxLength(50)
                .HasColumnName("фамилия");
        });

        modelBuilder.Entity<Преподаватель>(entity =>
        {
            entity.HasKey(e => e.IdПреподавателя).HasName("преподаватель_pkey");

            entity.ToTable("преподаватель");

            entity.Property(e => e.IdПреподавателя).HasColumnName("id_преподавателя");
            entity.Property(e => e.IdДолжности).HasColumnName("id_должности");
            entity.Property(e => e.IdИнститута).HasColumnName("id_института");
            entity.Property(e => e.IdКафедры).HasColumnName("id_кафедры");
            entity.Property(e => e.IdПользователя).HasColumnName("id_пользователя");

            entity.HasOne(d => d.IdДолжностиNavigation).WithMany(p => p.Преподавательs)
                .HasForeignKey(d => d.IdДолжности)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("преподаватель_id_должности_fkey");

            entity.HasOne(d => d.IdИнститутаNavigation).WithMany(p => p.Преподавательs)
                .HasForeignKey(d => d.IdИнститута)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("преподаватель_id_института_fkey");

            entity.HasOne(d => d.IdКафедрыNavigation).WithMany(p => p.Преподавательs)
                .HasForeignKey(d => d.IdКафедры)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("преподаватель_id_кафедры_fkey");

            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.Преподавательs)
                .HasForeignKey(d => d.IdПользователя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("преподаватель_id_пользователя_fkey");
        });

        modelBuilder.Entity<Профиль>(entity =>
        {
            entity.HasKey(e => e.IdПрофиля).HasName("профиль_pkey");

            entity.ToTable("профиль");

            entity.HasIndex(e => new { e.Название, e.IdНаправления }, "профиль_профиль_key").IsUnique();

            entity.Property(e => e.IdПрофиля).HasColumnName("id_профиля");
            entity.Property(e => e.IdНаправления).HasColumnName("id_направления");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");

            entity.HasOne(d => d.IdНаправленияNavigation).WithMany(p => p.Профильs)
                .HasForeignKey(d => d.IdНаправления)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("профиль_id_направления_fkey");
        });

        modelBuilder.Entity<Работа>(entity =>
        {
            entity.HasKey(e => e.IdРаботы).HasName("работа_pkey");

            entity.ToTable("работа");

            entity.HasIndex(e => e.Эцп, "работа_ЭЦП_key").IsUnique();

            entity.HasIndex(e => e.Местоположение, "работа_местоположение_key").IsUnique();

            entity.HasIndex(e => new { e.IdСтудента, e.Тема }, "работа_работа_студента_key").IsUnique();

            entity.HasIndex(e => new { e.Тема, e.IdТипаРаботы }, "работа_работа_студентов_key").IsUnique();

            entity.Property(e => e.IdРаботы).HasColumnName("id_работы");
            entity.Property(e => e.IdДолжности).HasColumnName("id_должности");
            entity.Property(e => e.IdДоступаРаботы).HasColumnName("id_доступа_работы");
            entity.Property(e => e.IdПреподавателя).HasColumnName("id_преподавателя");
            entity.Property(e => e.IdСтатусаРаботы).HasColumnName("id_статуса_работы");
            entity.Property(e => e.IdСтудента).HasColumnName("id_студента");
            entity.Property(e => e.IdТипаРаботы).HasColumnName("id_типа_работы");
            entity.Property(e => e.Аннотация).HasColumnName("аннотация");
            entity.Property(e => e.ДатаДобавления)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_добавления");
            entity.Property(e => e.ДатаИзменения)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("дата_изменения");
            entity.Property(e => e.КоличСтраниц).HasColumnName("колич_страниц");
            entity.Property(e => e.Местоположение)
                .HasColumnType("character varying")
                .HasColumnName("местоположение");
            entity.Property(e => e.Тема)
                .HasColumnType("character varying")
                .HasColumnName("тема");
            entity.Property(e => e.Эцп)
                .HasColumnType("character varying")
                .HasColumnName("ЭЦП");

            entity.HasOne(d => d.IdДолжностиNavigation).WithMany(p => p.Работаs)
                .HasForeignKey(d => d.IdДолжности)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("работа_id_должности_fkey");

            entity.HasOne(d => d.IdДоступаРаботыNavigation).WithMany(p => p.Работаs)
                .HasForeignKey(d => d.IdДоступаРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("работа_id_доступа_работы_fkey");

            entity.HasOne(d => d.IdПреподавателяNavigation).WithMany(p => p.Работаs)
                .HasForeignKey(d => d.IdПреподавателя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("работа_id_преподавателя_fkey");

            entity.HasOne(d => d.IdСтатусаРаботыNavigation).WithMany(p => p.Работаs)
                .HasForeignKey(d => d.IdСтатусаРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("работа_id_статуса_работы_fkey");

            entity.HasOne(d => d.IdСтудентаNavigation).WithMany(p => p.Работаs)
                .HasForeignKey(d => d.IdСтудента)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("работа_id_студента_fkey");

            entity.HasOne(d => d.IdТипаРаботыNavigation).WithMany(p => p.Работаs)
                .HasForeignKey(d => d.IdТипаРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("работа_id_типа_работы_fkey");
        });

        modelBuilder.Entity<Рецензент>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("рецензент_pkey");

            entity.ToTable("рецензент");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdДолжности).HasColumnName("id_должности");
            entity.Property(e => e.IdПреподавателя).HasColumnName("id_преподавателя");
            entity.Property(e => e.IdРаботы).HasColumnName("id_работы");

            entity.HasOne(d => d.IdДолжностиNavigation).WithMany(p => p.Рецензентs)
                .HasForeignKey(d => d.IdДолжности)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("рецензент_id_должности_fkey");

            entity.HasOne(d => d.IdПреподавателяNavigation).WithMany(p => p.Рецензентs)
                .HasForeignKey(d => d.IdПреподавателя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("рецензент_id_преподавателя_fkey");

            entity.HasOne(d => d.IdРаботыNavigation).WithMany(p => p.Рецензентs)
                .HasForeignKey(d => d.IdРаботы)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("рецензент_id_работы_fkey");
        });

        modelBuilder.Entity<РольПользователя>(entity =>
        {
            entity.HasKey(e => e.IdРоли).HasName("роль_пользователя_pkey");

            entity.ToTable("роль_пользователя");

            entity.HasIndex(e => e.Название, "роль_пользователя_название_key").IsUnique();

            entity.Property(e => e.IdРоли).HasColumnName("id_роли");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<СтатусРаботы>(entity =>
        {
            entity.HasKey(e => e.IdСтатусаРаботы).HasName("статус_работы_pkey");

            entity.ToTable("статус_работы");

            entity.HasIndex(e => e.Название, "статус_работы_название_key").IsUnique();

            entity.Property(e => e.IdСтатусаРаботы).HasColumnName("id_статуса_работы");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<СтатусЗаявления>(entity =>
        {
            entity.HasKey(e => e.IdСтатуса).HasName("статус_заявления_pkey");

            entity.ToTable("статус_заявления");
            entity.HasIndex(e => e.Название, "статус_заявления_название_key").IsUnique();

            entity.Property(e => e.IdСтатуса).HasColumnName("id_статуса");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<Студент>(entity =>
        {
            entity.HasKey(e => e.IdСтудента).HasName("студент_pkey");

            entity.ToTable("студент");

            entity.HasIndex(e => new { e.IdПользователя, e.IdИнститута, e.IdНаправления, e.IdПрофиля, e.IdФормыОбучения, e.IdУровняОбразования, e.ГодОкончания }, "студент_студент_key").IsUnique();

            entity.Property(e => e.IdСтудента).HasColumnName("id_студента");
            entity.Property(e => e.IdИнститута).HasColumnName("id_института");
            entity.Property(e => e.IdНаправления).HasColumnName("id_направления");
            entity.Property(e => e.IdПользователя).HasColumnName("id_пользователя");
            entity.Property(e => e.IdПрофиля).HasColumnName("id_профиля");
            entity.Property(e => e.IdУровняОбразования).HasColumnName("id_уровня_образования");
            entity.Property(e => e.IdФормыОбучения).HasColumnName("id_формы_обучения");
            entity.Property(e => e.ГодОкончания).HasColumnName("год_окончания");

            entity.HasOne(d => d.IdИнститутаNavigation).WithMany(p => p.Студентs)
                .HasForeignKey(d => d.IdИнститута)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("студент_id_института_fkey");

            entity.HasOne(d => d.IdНаправленияNavigation).WithMany(p => p.Студентs)
                .HasForeignKey(d => d.IdНаправления)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("студент_id_направления_fkey");

            entity.HasOne(d => d.IdПользователяNavigation).WithMany(p => p.Студентs)
                .HasForeignKey(d => d.IdПользователя)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("студент_id_пользователя_fkey");

            entity.HasOne(d => d.IdПрофиляNavigation).WithMany(p => p.Студентs)
                .HasForeignKey(d => d.IdПрофиля)
                .HasConstraintName("студент_id_профиля_fkey");

            entity.HasOne(d => d.IdУровняОбразованияNavigation).WithMany(p => p.Студентs)
                .HasForeignKey(d => d.IdУровняОбразования)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("студент_id_уровня_образования_fkey");

            entity.HasOne(d => d.IdФормыОбученияNavigation).WithMany(p => p.Студентs)
                .HasForeignKey(d => d.IdФормыОбучения)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("студент_id_формы_обучения_fkey");
        });

        modelBuilder.Entity<ТипРаботы>(entity =>
        {
            entity.HasKey(e => e.IdТипаРаботы).HasName("тип_работы_pkey");

            entity.ToTable("тип_работы");

            entity.HasIndex(e => e.Название, "тип_работы_название_key").IsUnique();

            entity.Property(e => e.IdТипаРаботы).HasColumnName("id_типа_работы");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<Угсн>(entity =>
        {
            entity.HasKey(e => e.IdУгсн).HasName("угсн_pkey");

            entity.ToTable("угсн", tb => tb.HasComment("угсн"));

            entity.HasIndex(e => e.Название, "угсн_название_key").IsUnique();

            entity.Property(e => e.IdУгсн).HasColumnName("id_угсн");
            entity.Property(e => e.IdУгснСтандарта).HasColumnName("id_угсн_стандарта");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");

            entity.HasOne(d => d.IdУгснСтандартаNavigation).WithMany(p => p.Угснs)
                .HasForeignKey(d => d.IdУгснСтандарта)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("угсн_id_угсн_стандарта_fkey");
        });

        modelBuilder.Entity<УгснСтандарт>(entity =>
        {
            entity.HasKey(e => e.IdУгснСтандарта).HasName("угсн_стандарт_pkey");

            entity.ToTable("угсн_стандарт", tb => tb.HasComment("Стандарт угсн"));

            entity.HasIndex(e => e.Название, "угсн_стандарт_название_key").IsUnique();

            entity.Property(e => e.IdУгснСтандарта).HasColumnName("id_угсн_стандарта");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<УровеньОбразования>(entity =>
        {
            entity.HasKey(e => e.IdУровняОбразования).HasName("уровень_образования_pkey");

            entity.ToTable("уровень_образования");

            entity.HasIndex(e => e.Название, "уровень_образования_название_key").IsUnique();

            entity.Property(e => e.IdУровняОбразования).HasColumnName("id_уровня_образования");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        modelBuilder.Entity<ФормаОбучения>(entity =>
        {
            entity.HasKey(e => e.IdФормыОбучения).HasName("форма_обучения_pkey");

            entity.ToTable("форма_обучения");

            entity.HasIndex(e => e.Название, "форма_обучения_название_key").IsUnique();

            entity.Property(e => e.IdФормыОбучения).HasColumnName("id_формы_обучения");
            entity.Property(e => e.Название)
                .HasColumnType("character varying")
                .HasColumnName("название");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
