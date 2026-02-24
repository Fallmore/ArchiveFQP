using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Archive.Migrations
{
    /// <inheritdoc />
    public partial class Children : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

			migrationBuilder.AddColumn<int>(
				name: "Детей",
				table: "аккаунт_пользователя",
				type: "integer",
				nullable: false,
				defaultValue: 0);
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.DropColumn(
				name: "Детей",
				table: "аккаунт_пользователя");
		}
    }
}
