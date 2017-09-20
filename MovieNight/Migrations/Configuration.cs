namespace MovieNight.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using MovieNight.Models;
    using System.Collections.Generic;
    using TMDbLib.Client;
    using TMDbLib.Objects.General;
    using TMDbLib.Objects.Movies;
    using TMDbLib.Objects.Search;

    internal sealed class Configuration : DbMigrationsConfiguration<MovieNight.Models.MovieNightDB>
    {
        //  This method will be called after migrating to the latest version
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MovieNight.Models.MovieNightDB context)
        {
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                System.Diagnostics.Debugger.Launch();
            }

            var user = new User { Username = "user", Password = "user", Role = Role.SimpleUser };
            var admin = new User { Username = "admin", Password = "admin", Role = Role.Admin };
            context.Users.AddOrUpdate(p => p.ID, user);
            context.Users.AddOrUpdate(p => p.ID, admin);
            context.SaveChanges();

            // TMDB API
            // TMDB API
            // TMDB API
            // TMDB API

            // Connectiong to TMDB API
            TMDbClient client = new TMDbClient("e77a93ac7dab813a39327cfaa10938e8");

            // Get the top rated movies
            TMDbLib.Objects.General.SearchContainer<TMDbLib.Objects.General.MovieResult> TopRatedMovies = client.GetMovieList(MovieListType.TopRated);

            // Get the now playing movies
            TMDbLib.Objects.General.SearchContainer<TMDbLib.Objects.General.MovieResult> NowPlayingMovies = client.GetMovieList(MovieListType.NowPlaying);

            // Get the popular movies
            TMDbLib.Objects.General.SearchContainer<TMDbLib.Objects.General.MovieResult> PopularMovies = client.GetMovieList(MovieListType.Popular);

            // Get the upcoming movies
            TMDbLib.Objects.General.SearchContainer<TMDbLib.Objects.General.MovieResult> UpComingMovies = client.GetMovieList(MovieListType.Upcoming);

            string BaseImgURL = "https://image.tmdb.org/t/p/w300";

            var Addedmovies = new List<MovieNight.Models.Movie>();

            // Goes over all of the now playing movies
            foreach (MovieResult currMovie in NowPlayingMovies.Results)
            {
                // Get the genre list into one string 
                List<Genre> genreList = client.GetMovie(currMovie.Id).Genres;

                // The new string value for the genre list
                string currMoviegenre = "";

                // Goes over all of the genres and add them to the new string
                foreach (Genre cuurGenre in genreList)
                {
                    // Add the current genre to new genre string
                    currMoviegenre += cuurGenre.Name + ", ";
                }

                // Trim trailing comma and white space
                currMoviegenre = currMoviegenre.TrimEnd(' ').TrimEnd(',');

                // Set movie trailer base path
                string MovieTralierBasePath = "https://www.youtube.com/embed/";

                string currMovieTrailerPath = "";

                if (client.GetMovie(currMovie.Id, MovieMethods.Videos).Videos.Results.Count() != 0)
                {
                    // Get the current movie video path
                    currMovieTrailerPath = client.GetMovie(currMovie.Id, MovieMethods.Videos).Videos.Results[0].Key;
                }

                // Get the current movie's director ID
                TMDbLib.Objects.General.Crew currdirector = client.GetMovieCredits(currMovie.Id).Crew.Where(crew1 => crew1.Job == "Director").ElementAt(0);

                // Get the current director object
                TMDbLib.Objects.People.Person director = client.GetPerson(currdirector.Id);

                // Create the current movie director object
                Director NewDirector = new Director
                {
                    Name = director.Name,
                    Gender = Gender.Male,
                    DateOfBirth = (director.Birthday == null ? DateTime.Parse("January 1, 1900") : (DateTime)director.Birthday.Value.Date),
                    Origin = (director.PlaceOfBirth == null ? "Unknown" : director.PlaceOfBirth.Split(',').Last()),
                    Picture = BaseImgURL + director.ProfilePath
                };

                // Adds the new director to directors DB
                context.Directors.Add(NewDirector);
//              context.SaveChanges();

                // Create the new movie object
                var NewMovie = new MovieNight.Models.Movie
                {
                    Title       = currMovie.Title,
                    Genre       = currMoviegenre,
                    Director    = NewDirector,
                    Plot        = currMovie.Overview,
                    Poster      = BaseImgURL + currMovie.PosterPath,
                    Rating      = currMovie.VoteAverage,
                    ReleaseDate = currMovie.ReleaseDate.Value.Date,
                    Trailer     = MovieTralierBasePath + currMovieTrailerPath
                };

                // Adds the new movie to movies DB
                context.Movies.Add(NewMovie);
                context.SaveChanges();
            }
            // TMDB API
            // TMDB API
            // TMDB API
            // TMDB API
        }
    }
}
