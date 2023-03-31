using Bogus;
using IM_DataAccess.Models;

namespace BogusData.Data;

public class DataGenerator
{
    Faker<User> userlFake;
    Faker<UserLogin> userLoginFake;
    public DataGenerator()
    {
        Randomizer.Seed = new Random(123);

        userlFake = new Faker<User>()
            //.RuleFor(u => u.Id, null)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
         ;


       


    }

    public User GeneratePerson()
    {
        return userlFake.Generate();
    }

    public IEnumerable<User> GeneratePeople()
    {
        return userlFake.GenerateForever();
    }

    //public IEnumerable<Incident> GenerateIncidents()
    //{
    //    List<string> titles = new List<string>()
    //    {
    //        "Crying hugging a dead snake, while recording it",
    //        "As Trump Awaits Charges, U.S. Enters Uncharted Territory",
    //        "What We Know About the Indictment of Donald Trump",
    //        "President Donald J. Trump sitting on his private plane listening as a reporter asks questions at a table in front of a row of windows.",
    //        "How a Trump-Era Rollback Mattered for Silicon Valley Bank’s Demise",
    //        "Even Donald Trump Should Be Held Accountable",
    //        "Thousands of Dollars for Something I Didn’t Do",
    //        "Google’s chief executive expressed worry and optimism about the A.I. race",
    //        "Large Tornado Seen in Arkansas as Storms Move Through Midwest and South",
    //        "The Fed’s War on Inflation Is a Class War",
    //        "There’s a Much Smarter Way to Take On TikTok",
    //        "Fallout From the Trump Indictment",
    //        "Death or Honor? For Millions of Russian Men, the Answer Is Simple.",
    //        "Retired and Buying a Starter Home. It’s Forever.",
    //        "Knitters Say Stitching Helps Them Follow the Thread in Meetings",
    //        "Kushner Firm Got Hundreds of Millions From 2 Persian Gulf Nations",
    //        "Amsterdam Has a Message for Male Tourists From Britain: ‘Stay Away’",
    //        "One of the Luckiest Lightning Strikes Ever Recorded",
    //        "We Are in a Golden Age for Hair",
    //        "He Bid $190 Million for the Flatiron Building, Then Didn’t Pay Up",
    //        "A Sibling Rivalry Divides a Famous Artist’s Legacy",
    //        "I Don’t Want to Let Them Down",
    //        "Why Tetris Consumed Your Brain",
    //        "After School Shooting, Nashville Grieves and Ponders Its Divisions",
    //        "Republican Senator Blocks Military Promotions Over Abortion Policy",
    //        "Bail Law Is a Key Stumbling Block as New York’s Budget Deadline Looms",
    //        "Republican Senator Blocks Military Promotions Over Abortion Policy",
    //        "Bail Law Is a Key Stumbling Block as New York’s Budget Deadline Loom",
    //        "The Fed’s Preferred Inflation Gauge Cooled Notably in February",
    //        "3 Great Nonfiction Films to Stream",
    //        "Tiny Love Stories: ‘Not an Ode to Breasts’",
    //        "Try the Upper East Side for ‘Dope’ Window Shopping",
    //        "Tiny Love Stories: ‘Not an Ode to Breasts’",
    //        "Why the Yankees Are Requesting a Change to a Baseball Tradition",
    //        "LeBron James Opened a Starbucks. It’s Providing Much More Than Coffee.",
    //        "‘The Godfather’ Behind the N.F.L.’s Best Defenses",
    //        "Wirecutter’s Most Popular Picks of March",
    //        "These 6 Wirecutter Picks Are on Sale Right Now",
    //        "A day after a grand jury voted to indict Mr. Trump",
    //        "Republicans continued to criticize the Manhattan district attorney",
    //        "Mr. Trump is likely to be arraigned on Tuesday",
    //        "A lawyer for the former president said that he would not take a plea deal",
    //        "One participant in the courtroom during that time was unusually equipped",
    //        "The presiding judge, Juan M. Merchan, who as a young man",
    //        "The editorial pages of news organizations around the world weighed in",
    //        "Some lauded the move as a necessary step to holding Mr. Trump accountable",
    //        "The judge who oversaw the tax fraud case against Trump’s business is expected to preside over his arraignment.",
    //        "Here are the prominent arguments for and against the indictment.",
    //        "I asked Steven Zeidman, a professor who is the director of the criminal defense clinic at City University of New York School of Law",
    //        "McConnell is among few high-profile Republicans to stay silent on Trump’s indictment.",
    //        "Ivanka Trump, after hours of silence, issues a muted statement: ‘I love my father.’",
    //        "Minimum Requirements: .NET Standard 1.3 or .NET Standard 2.0",
    //    };


    //    Faker<Incident> incidentFake;
    //    incidentFake = new Faker<Incident>()
    //       //.RuleFor(u => u.Id, null)
    //       .RuleFor(u => u.Title, f => f. .PickRandom(titles))
    //       .RuleFor(u => u.LastName, f => f.Name.LastName())
    //       .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
    //       .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
    //    ;
    //    return userlFake.GenerateForever();
    //}


}
