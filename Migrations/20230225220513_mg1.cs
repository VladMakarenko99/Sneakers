// using Microsoft.EntityFrameworkCore.Metadata;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace practice.Migrations
// {
//     /// <inheritdoc />
//     public partial class mg1 : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.AlterDatabase()
//                 .Annotation("MySql:CharSet", "utf8mb4");


//             migrationBuilder.CreateTable(
//                 name: "Users",
//                 columns: table => new
//                 {
//                     Id = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
//                     FirstName = table.Column<string>(type: "longtext", nullable: true)
//                         .Annotation("MySql:CharSet", "utf8mb4"),
//                     LastName = table.Column<string>(type: "longtext", nullable: true)
//                         .Annotation("MySql:CharSet", "utf8mb4"),
//                     Email = table.Column<string>(type: "longtext", nullable: true)
//                         .Annotation("MySql:CharSet", "utf8mb4"),
//                     Password = table.Column<string>(type: "longtext", nullable: true)
//                         .Annotation("MySql:CharSet", "utf8mb4"),
//                     ConfirmedPass = table.Column<string>(type: "longtext", nullable: true)
//                         .Annotation("MySql:CharSet", "utf8mb4"),
//                     Salt = table.Column<string>(type: "longtext", nullable: true)
//                         .Annotation("MySql:CharSet", "utf8mb4")
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Users", x => x.Id);
//                 })
//                 .Annotation("MySql:CharSet", "utf8mb4");


            
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {


//             migrationBuilder.DropTable(
//                 name: "Users");

//         }
//     }
// }
