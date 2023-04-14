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

    public IEnumerable<Incident> GenerateIncidents(IEnumerable<User> users)
    {
        List<string> titles = new List<string>()
        {
            "Crying hugging a dead snake, while recording it",
            "As Trump Awaits Charges, U.S. Enters Uncharted Territory",
            "What We Know About the Indictment of Donald Trump",
            "President Donald J. Trump sitting on his private plane listening as a reporter asks questions at a table in front of a row of windows.",
            "How a Trump-Era Rollback Mattered for Silicon Valley Bank’s Demise",
            "Even Donald Trump Should Be Held Accountable",
            "Thousands of Dollars for Something I Didn’t Do",
            "Google’s chief executive expressed worry and optimism about the A.I. race",
            "Large Tornado Seen in Arkansas as Storms Move Through Midwest and South",
            "The Fed’s War on Inflation Is a Class War",
            "There’s a Much Smarter Way to Take On TikTok",
            "Fallout From the Trump Indictment",
            "Death or Honor? For Millions of Russian Men, the Answer Is Simple.",
            "Retired and Buying a Starter Home. It’s Forever.",
            "Knitters Say Stitching Helps Them Follow the Thread in Meetings",
            "Kushner Firm Got Hundreds of Millions From 2 Persian Gulf Nations",
            "Amsterdam Has a Message for Male Tourists From Britain: ‘Stay Away’",
            "One of the Luckiest Lightning Strikes Ever Recorded",
            "We Are in a Golden Age for Hair",
            "He Bid $190 Million for the Flatiron Building, Then Didn’t Pay Up",
            "A Sibling Rivalry Divides a Famous Artist’s Legacy",
            "I Don’t Want to Let Them Down",
            "Why Tetris Consumed Your Brain",
            "After School Shooting, Nashville Grieves and Ponders Its Divisions",
            "Republican Senator Blocks Military Promotions Over Abortion Policy",
            "Bail Law Is a Key Stumbling Block as New York’s Budget Deadline Looms",
            "Republican Senator Blocks Military Promotions Over Abortion Policy",
            "Bail Law Is a Key Stumbling Block as New York’s Budget Deadline Loom",
            "The Fed’s Preferred Inflation Gauge Cooled Notably in February",
            "3 Great Nonfiction Films to Stream",
            "Tiny Love Stories: ‘Not an Ode to Breasts’",
            "Try the Upper East Side for ‘Dope’ Window Shopping",
            "Tiny Love Stories: ‘Not an Ode to Breasts’",
            "Why the Yankees Are Requesting a Change to a Baseball Tradition",
            "LeBron James Opened a Starbucks. It’s Providing Much More Than Coffee.",
            "‘The Godfather’ Behind the N.F.L.’s Best Defenses",
            "Wirecutter’s Most Popular Picks of March",
            "These 6 Wirecutter Picks Are on Sale Right Now",
            "A day after a grand jury voted to indict Mr. Trump",
            "Republicans continued to criticize the Manhattan district attorney",
            "Mr. Trump is likely to be arraigned on Tuesday",
            "A lawyer for the former president said that he would not take a plea deal",
            "One participant in the courtroom during that time was unusually equipped",
            "The presiding judge, Juan M. Merchan, who as a young man",
            "The editorial pages of news organizations around the world weighed in",
            "Some lauded the move as a necessary step to holding Mr. Trump accountable",
            "The judge who oversaw the tax fraud case against Trump’s business is expected to preside over his arraignment.",
            "Here are the prominent arguments for and against the indictment.",
            "I asked Steven Zeidman, a professor who is the director of the criminal defense clinic at City University of New York School of Law",
            "McConnell is among few high-profile Republicans to stay silent on Trump’s indictment.",
            "Ivanka Trump, after hours of silence, issues a muted statement: ‘I love my father.’",
            "Minimum Requirements: .NET Standard 1.3 or .NET Standard 2.0",
            "How Alvin Bragg Resurrected the Case Against Donald Trump",
            "How Donald Trump’s Playboy Persona Came Back to Haunt Him",
            "Many of Donald Trump’s G.O.P. rivals snapped into line behind him, showing how difficult it may be to oppose him in 2024.",
            "Storms Kill at Least 21 as Tornadoes Tear Through Midwest and South",
            "In Illinois, a theater’s roof collapsed during a packed heavy metal concert, killing one.",
            "China Draws Lessons From Russia’s Losses in Ukraine, and Its Gains\r\n",
            "Analysis: Espionage Charge Complicates Effort to Free Reporter in Russia",
            "Kyiv charged a senior religious leader with supporting Moscow’s war effort, as Russia pounded eastern Ukraine.",
            "Inside the F.B.I.’s Jan. 6 Investigation of the Proud Boys",
            "As the Final Four Finishes, a Parallel Transfer Season Plays Out, Too",
            "How Iowa Ended South Carolina’s Storied Perfect Season",
            "Hold the champagne. Athletes are celebrating success in sports by throwing water over each other.",
            "The ChatGPT King Isn’t Worried, but He Knows You Might Be",
            "Thailand’s Unemployed Elephants Are Back Home, Huge and Hungry",
            "The Weekender: An Old English Manor Needs Some Modern Love",
            "Did you follow the news this week? Take our quiz.",
            "The Best Movies and Shows on Hulu Right Now",
            "How a Federal Judge’s Ruling on Obamacare Could Change Health Insurance",
            "Small Businesses Look for Safety Before the Next Bank Crisis",
            "Top Economist Leaves White House, and an Economy Not Yet ‘Normal’",
            "Margot Stern Strom, Anti-Bigotry Educator, Dies at 81",
            "The N.B.A. and Its Players Union Reach a Tentative Labor Deal",
            "Inside the Downfall of a College Basketball Power",
            "From Camp Nou to ‘Magic Mountain’: This is Barcelona’s Temporary Home",
            "M.L.B. Commissioner Explains Why the Analytics ‘Arms Race’ Damaged Baseball",
            "The Warriors’ Future Is Unknown, but There’s Hope This Isn’t the Last Dance",
            "These 6 Wirecutter Picks Are on Sale Right Now",
            "Wirecutter’s Most Popular Picks of March",
            "What they were doing, new interviews show, was going back to square one",
            "For a time, their efforts were haphazard as they examined a wide range of Mr. Trump’s business practices",
            "Halfway through the meal, he held up his iPhone so I could see the contract",
            "Later, as Mr. Altman sipped a sweet wine in lieu of dessert, he compared his company to the Manhattan Project.",
            "He believed A.G.I. would bring the world prosperity and wealth like no one had ever seen. He also worried that the technologies his company was building could cause serious harm",
            "In 2019, this sounded like science fiction.",
            "In 2023, people are beginning to wonder if Sam Altman was more prescient than they realized.",
            "Now that OpenAI has released an online chatbot called ChatGPT",
            "As people realize that this technology is also a way of spreading falsehoods or even persuading people to do things",
            "This past week, more than a thousand A.I. experts and tech leaders called on OpenAI and other companies to pause their work on systems like ChatGPT",
            "And yet, when people act as if Mr. Altman has nearly realized his long-held vision, he pushes back.",
            "Many industry leaders, A.I. researchers and pundits see ChatGPT as a fundamental technological shift",
            "Some believe it will deliver a utopia where everyone has all the time and money ever needed.",
            "Mr. Altman, a slim, boyish-looking, 37-year-old entrepreneur",
            "That means he is often criticized from all directions. But those closest to him believe this is as it should be. “If you’re equally upsetting both extreme sides, then you’re doing something right,” said OpenAI’s president,",
            "He believes that artificial intelligence will happen one way or another",
            "It’s an attitude that mirrors Mr. Altman’s own trajectory.",
            "His life has been a fairly steady climb toward greater prosperity and wealth",
            "But if he’s wrong, there’s an escape hatch",
            "The warning, sent with the driving directions, was: “Watch out for cows.”",
            "Mr. Altman’s weekend home is a ranch in Napa, Calif., where farmhands grow wine grapes and raise cattle.",
            "As you approach the property, the cows roam across both the green fields and gravel roads.",
            "Mr. Altman is a man who lives with contradictions, even at his getaway home",
            "On a recent afternoon walk at the ranch, we stopped to rest at the edge of a small lake.",
        };

        //var fileText = File.ReadAllText(@"d:\longfile.txt");
        //string[] lines = fileText.Split('.');

        List<string> statuses = new List<string>()
        {
            "N","I","C", "A"
        };

        List<int> num = new List<int>()     { -1,1,2,3,4,5,6,7,8,9,10,11,12, -2,-3,-4 };
        List<int> positiveNum = new List<int>() { 0,1, 2, 3 };

        Faker<Incident> incidentFake;
        incidentFake = new Faker<Incident>()
             //.RuleFor(u => u.Id, null)
           .RuleFor(u => u.CreatedAT, f => DateTime.UtcNow.AddMonths(f.PickRandom(positiveNum)))
           .RuleFor(u => u.Title, f => f.PickRandom(titles))
           .RuleFor(u => u.Description, f => f.PickRandom(titles)+ ". " + f.PickRandom(titles))
           .RuleFor(u => u.AdditionalData, f => f.PickRandom(titles) + ". " + f.PickRandom(titles))
           .RuleFor(u => u.Status, f => f.PickRandom(statuses))
           .RuleFor(u => u.AssignedTo, f => f.PickRandom(users).Id)
           .RuleFor(u => u.CreatedBy, f => f.PickRandom(users).Id)
           .RuleFor(u => u.StartTime, f => DateTime.UtcNow.AddMonths(f.PickRandom(num)))
           .RuleFor(u => u.DueDate, f =>  DateTime.UtcNow.AddMonths(f.PickRandom(num)))

        ;
        return incidentFake.GenerateForever();
    }

    public IEnumerable<Message> GenerateMessages(IEnumerable<User> users)
    {
        List<string> titles = new List<string>()
        {
            "Crying hugging a dead snake, while recording it",
            "As Trump Awaits Charges, U.S. Enters Uncharted Territory",
            "What We Know About the Indictment of Donald Trump",
            "President Donald J. Trump sitting on his private plane listening as a reporter asks questions at a table in front of a row of windows.",
            "How a Trump-Era Rollback Mattered for Silicon Valley Bank’s Demise",
            "Even Donald Trump Should Be Held Accountable",
            "Thousands of Dollars for Something I Didn’t Do",
            "Google’s chief executive expressed worry and optimism about the A.I. race",
            "Large Tornado Seen in Arkansas as Storms Move Through Midwest and South",
            "The Fed’s War on Inflation Is a Class War",
            "There’s a Much Smarter Way to Take On TikTok",
            "Fallout From the Trump Indictment",
            "Death or Honor? For Millions of Russian Men, the Answer Is Simple.",
            "Retired and Buying a Starter Home. It’s Forever.",
            "Knitters Say Stitching Helps Them Follow the Thread in Meetings",
            "Kushner Firm Got Hundreds of Millions From 2 Persian Gulf Nations",
            "Amsterdam Has a Message for Male Tourists From Britain: ‘Stay Away’",
            "One of the Luckiest Lightning Strikes Ever Recorded",
            "We Are in a Golden Age for Hair",
            "He Bid $190 Million for the Flatiron Building, Then Didn’t Pay Up",
            "A Sibling Rivalry Divides a Famous Artist’s Legacy",
            "I Don’t Want to Let Them Down",
            "Why Tetris Consumed Your Brain",
            "After School Shooting, Nashville Grieves and Ponders Its Divisions",
            "Republican Senator Blocks Military Promotions Over Abortion Policy",
            "Bail Law Is a Key Stumbling Block as New York’s Budget Deadline Looms",
            "Republican Senator Blocks Military Promotions Over Abortion Policy",
            "Bail Law Is a Key Stumbling Block as New York’s Budget Deadline Loom",
            "The Fed’s Preferred Inflation Gauge Cooled Notably in February",
            "3 Great Nonfiction Films to Stream",
            "Tiny Love Stories: ‘Not an Ode to Breasts’",
            "Try the Upper East Side for ‘Dope’ Window Shopping",
            "Tiny Love Stories: ‘Not an Ode to Breasts’",
            "Why the Yankees Are Requesting a Change to a Baseball Tradition",
            "LeBron James Opened a Starbucks. It’s Providing Much More Than Coffee.",
            "‘The Godfather’ Behind the N.F.L.’s Best Defenses",
            "Wirecutter’s Most Popular Picks of March",
            "These 6 Wirecutter Picks Are on Sale Right Now",
            "A day after a grand jury voted to indict Mr. Trump",
            "Republicans continued to criticize the Manhattan district attorney",
            "Mr. Trump is likely to be arraigned on Tuesday",
            "A lawyer for the former president said that he would not take a plea deal",
            "One participant in the courtroom during that time was unusually equipped",
            "The presiding judge, Juan M. Merchan, who as a young man",
            "The editorial pages of news organizations around the world weighed in",
            "Some lauded the move as a necessary step to holding Mr. Trump accountable",
            "The judge who oversaw the tax fraud case against Trump’s business is expected to preside over his arraignment.",
            "Here are the prominent arguments for and against the indictment.",
            "I asked Steven Zeidman, a professor who is the director of the criminal defense clinic at City University of New York School of Law",
            "McConnell is among few high-profile Republicans to stay silent on Trump’s indictment.",
            "Ivanka Trump, after hours of silence, issues a muted statement: ‘I love my father.’",
            "Minimum Requirements: .NET Standard 1.3 or .NET Standard 2.0",
            "How Alvin Bragg Resurrected the Case Against Donald Trump",
            "How Donald Trump’s Playboy Persona Came Back to Haunt Him",
            "Many of Donald Trump’s G.O.P. rivals snapped into line behind him, showing how difficult it may be to oppose him in 2024.",
            "Storms Kill at Least 21 as Tornadoes Tear Through Midwest and South",
            "In Illinois, a theater’s roof collapsed during a packed heavy metal concert, killing one.",
            "China Draws Lessons From Russia’s Losses in Ukraine, and Its Gains\r\n",
            "Analysis: Espionage Charge Complicates Effort to Free Reporter in Russia",
            "Kyiv charged a senior religious leader with supporting Moscow’s war effort, as Russia pounded eastern Ukraine.",
            "Inside the F.B.I.’s Jan. 6 Investigation of the Proud Boys",
            "As the Final Four Finishes, a Parallel Transfer Season Plays Out, Too",
            "How Iowa Ended South Carolina’s Storied Perfect Season",
            "Hold the champagne. Athletes are celebrating success in sports by throwing water over each other.",
            "The ChatGPT King Isn’t Worried, but He Knows You Might Be",
            "Thailand’s Unemployed Elephants Are Back Home, Huge and Hungry",
            "The Weekender: An Old English Manor Needs Some Modern Love",
            "Did you follow the news this week? Take our quiz.",
            "The Best Movies and Shows on Hulu Right Now",
            "How a Federal Judge’s Ruling on Obamacare Could Change Health Insurance",
            "Small Businesses Look for Safety Before the Next Bank Crisis",
            "Top Economist Leaves White House, and an Economy Not Yet ‘Normal’",
            "Margot Stern Strom, Anti-Bigotry Educator, Dies at 81",
            "The N.B.A. and Its Players Union Reach a Tentative Labor Deal",
            "Inside the Downfall of a College Basketball Power",
            "From Camp Nou to ‘Magic Mountain’: This is Barcelona’s Temporary Home",
            "M.L.B. Commissioner Explains Why the Analytics ‘Arms Race’ Damaged Baseball",
            "The Warriors’ Future Is Unknown, but There’s Hope This Isn’t the Last Dance",
            "These 6 Wirecutter Picks Are on Sale Right Now",
            "Wirecutter’s Most Popular Picks of March",
            "What they were doing, new interviews show, was going back to square one",
            "For a time, their efforts were haphazard as they examined a wide range of Mr. Trump’s business practices",
            "Halfway through the meal, he held up his iPhone so I could see the contract",
            "Later, as Mr. Altman sipped a sweet wine in lieu of dessert, he compared his company to the Manhattan Project.",
            "He believed A.G.I. would bring the world prosperity and wealth like no one had ever seen. He also worried that the technologies his company was building could cause serious harm",
            "In 2019, this sounded like science fiction.",
            "In 2023, people are beginning to wonder if Sam Altman was more prescient than they realized.",
            "Now that OpenAI has released an online chatbot called ChatGPT",
            "As people realize that this technology is also a way of spreading falsehoods or even persuading people to do things",
            "This past week, more than a thousand A.I. experts and tech leaders called on OpenAI and other companies to pause their work on systems like ChatGPT",
            "And yet, when people act as if Mr. Altman has nearly realized his long-held vision, he pushes back.",
            "Many industry leaders, A.I. researchers and pundits see ChatGPT as a fundamental technological shift",
            "Some believe it will deliver a utopia where everyone has all the time and money ever needed.",
            "Mr. Altman, a slim, boyish-looking, 37-year-old entrepreneur",
            "That means he is often criticized from all directions. But those closest to him believe this is as it should be. “If you’re equally upsetting both extreme sides, then you’re doing something right,” said OpenAI’s president,",
            "He believes that artificial intelligence will happen one way or another",
            "It’s an attitude that mirrors Mr. Altman’s own trajectory.",
            "His life has been a fairly steady climb toward greater prosperity and wealth",
            "But if he’s wrong, there’s an escape hatch",
            "The warning, sent with the driving directions, was: “Watch out for cows.”",
            "Mr. Altman’s weekend home is a ranch in Napa, Calif., where farmhands grow wine grapes and raise cattle.",
            "As you approach the property, the cows roam across both the green fields and gravel roads.",
            "Mr. Altman is a man who lives with contradictions, even at his getaway home",
            "On a recent afternoon walk at the ranch, we stopped to rest at the edge of a small lake.",
        };

        List<string> statuses = new List<string>()
        {
            "N","I","C", "A"
        };

        List<int> num = new List<int>()
        {
            -1,1,2,3,4,5,6,7,8,9,10,11,12, -2,-3,-4

        };

        Faker<Message> messageFake;
        messageFake = new Faker<Message>()
           //.RuleFor(u => u.Id, null)
           .RuleFor(m => m.MessageText, f => f.PickRandom(titles))
           .RuleFor(m => m.From, f => f.PickRandom(users).Id)
           .RuleFor(m => m.To, f => f.PickRandom(users).Id)
           //.RuleFor(m => m.Status,f => "Unread")
          ;
        return messageFake.GenerateForever();
    }


}
