namespace DataLayer
{
    public class BookManagerContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookComment> BookComments { get; set; }
        public DbSet<BookRating> BookRatings { get; set; }
        public DbSet<BookRequest> BookRequests { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Publisher> Publisher { get; set; }
        public DbSet<ReadingLog> ReadingLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserBook> UsersBook { get; set; }
        public DbSet<UserRestriction> UsersRestriction { get; set; }

        public BookManagerContext() { }

        public BookManagerContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable("Authors");

                entity.Property(a => a.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(a => a.Biography)
                      .HasMaxLength(1000);

                entity.HasMany(a => a.Books)
                      .WithOne(b => b.Author)
                      .HasForeignKey(b => b.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("Books");

                entity.HasIndex(b => b.ISBN).IsUnique();

                entity.Property(b => b.ISBN)
                      .HasMaxLength(13)
                      .IsRequired();

                entity.Property(b => b.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(b => b.Cover)
                      .IsRequired();

                entity.Property(b => b.Description)
                      .HasMaxLength(500);

                entity.HasOne(b => b.Genre)
                      .WithMany(g => g.Books)
                      .HasForeignKey(b => b.GenreId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(b => b.Publisher)
                      .WithMany(p => p.Books)
                      .HasForeignKey(b => b.PublisherId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(b => b.UserBooks)
                      .WithOne(ub => ub.Book)
                      .HasForeignKey(ub => ub.BookId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(b => b.Ratings)
                      .WithOne(r => r.Book)
                      .HasForeignKey(r => r.BookId)
                      .OnDelete(DeleteBehavior.ClientCascade);

                entity.HasMany(b => b.Comments)
                      .WithOne(c => c.Book)
                      .HasForeignKey(c => c.BookId)
                      .OnDelete(DeleteBehavior.ClientCascade);
            });

            modelBuilder.Entity<BookComment>(entity =>
            {
                entity.ToTable("BookComments",t =>
                {
                    t.HasCheckConstraint("CK_BookComment_PageProgress_Positive", "UserPageProgress >= 1");
                });

                entity.Property(c => c.Comment)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.HasOne(c => c.User)
                      .WithMany(u => u.BookComments)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BookRating>(entity =>
            {
                entity.ToTable("BookRatings",t =>
                {
                    t.HasCheckConstraint("CK_BookRating_ValidRange", "Rating >= 1 AND Rating <= 10");
                });

                entity.HasOne(r => r.User)
                      .WithMany(u => u.BookRatings)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<BookRequest>(entity =>
            {
                entity.ToTable("BookRequests");

                entity.Property(r => r.ISBN)
                      .HasMaxLength(13)
                      .IsRequired();

                entity.Property(r => r.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(r => r.RequestDescription)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(r => r.DateSent)
                      .IsRequired();

                entity.Property(r => r.Status)
                      .HasConversion<byte>()
                      .IsRequired();

                entity.HasOne(r => r.Sender)
                      .WithMany()
                      .HasForeignKey(r => r.SenderId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(r => r.ActionedBy)
                      .WithMany()
                      .HasForeignKey(r => r.ActionedById)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.ToTable("Genres");

                entity.HasIndex(g => g.Name).IsUnique();

                entity.Property(g => g.Name)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(g => g.Description)
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.ToTable("Publishers");

                entity.HasIndex(p => p.Name).IsUnique();

                entity.Property(p => p.Name)
                      .HasMaxLength(70)
                      .IsRequired();

                entity.Property(p => p.Description)
                      .HasMaxLength(500);

                entity.Property(p => p.Website)
                      .HasMaxLength(200);
            });

            modelBuilder.Entity<ReadingLog>(entity =>
            {
                entity.ToTable("ReadingLogs",t =>
                {
                    t.HasCheckConstraint("CK_ReadingLog_StartingPage_Positive", "StartingPage >= 0");
                    t.HasCheckConstraint("CK_ReadingLog_EndingPage_GTE_StartingPage", "EndingPage >= StartingPage");
                });

                entity.HasOne(r => r.UserBook)
                      .WithMany(ub => ub.ReadingLogs)
                      .HasForeignKey(r => r.UserBookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasIndex(u => u.EmailAddress).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();

                entity.Property(u => u.Username)
                      .HasMaxLength(40)
                      .IsRequired();

                entity.Property(u => u.EmailAddress)
                      .IsRequired();

                entity.Property(u => u.PasswordHash)
                      .IsRequired();

                entity.Property(u => u.Role)
                      .HasConversion<byte>()
                      .IsRequired();

                entity.HasMany(u => u.UserBooks)
                      .WithOne(ub => ub.User)
                      .HasForeignKey(ub => ub.UserId)
                      .OnDelete(DeleteBehavior.ClientCascade);

                entity.HasMany(u => u.UserRestrictions)
                      .WithOne(ur => ur.User)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserBook>(entity =>
            {
                entity.ToTable("UserBooks");

                entity.Property(ub => ub.Status)
                      .HasConversion<byte>()
                      .IsRequired();
            });

            modelBuilder.Entity<UserRestriction>(entity =>
            {
                entity.Property(ur => ur.StartDate)
                      .IsRequired();

                entity.ToTable("UserRestrictions", t =>
                {
                    t.HasCheckConstraint(
                        "CK_UserRestriction_EndDate_GTE_StartDate",
                        "EndDate IS NULL OR EndDate > StartDate");
                });

                entity.Property(ur => ur.Reason)
                      .HasMaxLength(500);
            });

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=BookManagerDB;Trusted_Connection=True;");
            }
            base.OnConfiguring(optionsBuilder);
        }
    }
}
