using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SFM;

public class AppDbContext : DbContext
{
    public DbSet<Npc> Npcs { get; set; } = null!;
    public DbSet<DialogueGraph> Dialogues { get; set; } = null!;
    public DbSet<Node> Nodes { get; set; } = null!;
    public DbSet<Connection> Connections { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DialogueGraph>().HasMany(d => d.Nodes).WithOne().OnDelete(DeleteBehavior.Cascade);
    }
}