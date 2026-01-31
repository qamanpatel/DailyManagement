using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostingLayer",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DurabilitySpec",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiberBy",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FiberStartDate",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialNo",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialSpec",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelingBy",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModelingLastDate",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderBy",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaintSpec",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualitySpec",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UOM",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkNatureSpec",
                table: "Orders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CostingLayer",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DurabilitySpec",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FiberBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FiberStartDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MaterialNo",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MaterialSpec",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ModelingBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ModelingLastDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaintSpec",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QualitySpec",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UOM",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WorkNatureSpec",
                table: "Orders");
        }
    }
}
