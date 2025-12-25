# EasyLearn - Online Learning Platform

A comprehensive online learning platform built with ASP.NET Core 9.0, featuring role-based access control for Admins, Instructors, and Students.

## 🚀 Features

### Admin Features
- **Dashboard**: View platform statistics and analytics
- **User Management**: Manage students and instructors with search and filtering
- **Course Approval**: Review and approve instructor-submitted courses
- **Category Management**: Create and manage course categories
- **Platform Analytics**: Monitor enrollments and user activity
- **Announcements**: Create platform-wide announcements
- **Data Export**: Export users and courses data to CSV
- **Account Management**: Activate/deactivate user accounts

### Instructor Features
- **Dashboard**: View course statistics, earnings, and student enrollment
- **Course Creation**: Create and manage courses with pricing (free/paid)
- **Content Management**: Upload videos via YouTube links and study materials
- **Lesson Management**: Organize course content into structured lessons
- **Quiz Creation**: Create assessments with multiple question types (MCQ, True/False)
- **Student Performance**: Track student progress and quiz results
- **Earnings Tracking**: Monitor revenue from paid courses

### Student Features
- **Course Browsing**: Search and filter available courses by category
- **Course Preview**: View course details, ratings, and reviews before enrollment
- **Enrollment**: Enroll in free courses or paid courses (with payment integration ready)
- **Video Learning**: Watch YouTube video lectures with progress tracking
- **Material Downloads**: Access and download course materials
- **Progress Tracking**: Monitor learning progress with visual indicators
- **Quiz Taking**: Attempt course assessments with instant results
- **Reviews & Ratings**: Rate and review completed courses
- **Certificates**: Download PDF certificates upon course completion
- **Dashboard**: View enrolled courses, progress, and achievements

## 🛠 Technology Stack

- **Backend**: ASP.NET Core 9.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Frontend**: Bootstrap 5, Font Awesome, jQuery
- **PDF Generation**: iTextSharp for certificates
- **Architecture**: MVC Pattern with service layer
- **Security**: Role-based access control, secure authentication

## 📋 Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Git (for cloning)

## 🔧 Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd EasyLearn
```

### 2. Restore NuGet Packages
```bash
dotnet restore
```




### 4. Create and Seed Database
The application will automatically create the database and seed initial data on first run.

### 5. Run the Application
```bash
dotnet run
```

### 6. Access the Application
Open your browser and navigate to `https://`

## 👥 Default User Accounts

The system creates default accounts for testing:





### Core Entities
- **ApplicationUser**: Extended Identity user with role-based access
- **Category**: Course categorization system
- **Course**: Main course entity with instructor and category relationships
- **Lesson**: Individual lessons within courses with video and material links
- **Quiz/Question/Answer**: Comprehensive assessment system
- **Enrollment**: Student-course relationships with progress tracking
- **Review**: Course rating and review system
- **Certificate**: Digital certificate generation and management
- **Announcement**: Platform-wide announcements
- **Progress Tracking**: Detailed lesson and course completion tracking

### Key Relationships
- Users have roles (Admin, Instructor, Student)
- Instructors create multiple courses
- Students enroll in multiple courses
- Courses contain lessons, quizzes, and receive reviews
- Progress is tracked per student per lesson/course
- Certificates are issued upon course completion

## 🔐 Security Features

- **Role-based Authorization**: Different access levels for each user type
- **Secure Authentication**: ASP.NET Core Identity with password policies
- **Data Protection**: Entity Framework with proper relationships and constraints
- **Input Validation**: Server-side validation on all forms
- **Password Reset**: Secure password reset functionality
- **Account Management**: Admin-controlled account activation/deactivation

## 🌐 API Endpoints

### Authentication
- `GET/POST /Account/Login` - User login
- `GET/POST /Account/Register` - User registration
- `GET/POST /Account/ForgotPassword` - Password reset request
- `GET/POST /Account/ResetPassword` - Password reset completion
- `POST /Account/Logout` - User logout

### Admin Routes (Requires Admin role)
- `GET /Admin` - Admin dashboard with analytics
- `GET /Admin/ManageUsers` - User management with search/filter
- `GET /Admin/ApproveCourses` - Course approval interface
- `POST /Admin/ApproveCourse/{id}` - Approve specific course
- `POST /Admin/RejectCourse/{id}` - Reject specific course
- `GET /Admin/ManageCategories` - Category management
- `GET /Admin/ManageAnnouncements` - Announcement management
- `GET /Admin/ExportUsers` - Export user data (CSV)
- `GET /Admin/ExportCourses` - Export course data (CSV)

### Instructor Routes (Requires Instructor role)
- `GET /Instructor` - Instructor dashboard with earnings
- `GET /Instructor/MyCourses` - Course management interface
- `GET/POST /Instructor/CreateCourse` - Course creation
- `GET /Instructor/ManageLessons/{courseId}` - Lesson management
- `POST /Instructor/CreateLesson` - Create new lesson
- `GET /Instructor/ManageQuizzes/{courseId}` - Quiz management
- `POST /Instructor/CreateQuiz` - Create quiz with questions
- `GET /Instructor/StudentPerformance/{courseId}` - Student analytics

### Student Routes (Requires Student role)
- `GET /Student` - Student dashboard with progress
- `GET /Student/BrowseCourses` - Course catalog with search/filter
- `GET /Student/CourseDetails/{id}` - Detailed course information
- `POST /Student/EnrollCourse` - Course enrollment
- `GET /Student/MyCourses` - Enrolled courses overview
- `GET /Student/WatchLesson/{lessonId}` - Video learning interface
- `POST /Student/CompleteLesson/{lessonId}` - Mark lesson as completed
- `GET /Student/TakeQuiz/{quizId}` - Quiz taking interface
- `POST /Student/SubmitQuiz` - Quiz submission and grading
- `POST /Student/AddReview` - Course review submission
- `GET /Student/DownloadCertificate/{courseId}` - Certificate download

## 🎨 Customization

### Adding New Features
1. Create new models in `/Models`
2. Update `ApplicationDbContext` with new DbSets
3. Create controllers with appropriate authorization
4. Add views with consistent Bootstrap styling
5. Update navigation in `_Layout.cshtml`

### Styling Customization
- Main styles in `/wwwroot/css/site.css`
- Bootstrap 5 classes used throughout
- Font Awesome icons for UI elements
- Responsive design for mobile compatibility
- Custom CSS classes for course cards, progress indicators

### Business Logic Services
- `ICertificateService` - Certificate generation and PDF creation
- `IProgressService` - Progress tracking and course completion
- Services are registered in `Program.cs` for dependency injection

## 🚀 Deployment

### Production Considerations
1. Update connection string for production database
2. Configure proper authentication settings
3. Enable HTTPS and security headers
4. Set up proper logging and monitoring
5. Configure file upload storage for course materials
6. Set up email service for password reset notifications
7. Configure payment gateway for paid courses

### Environment Variables
- `ConnectionStrings:DefaultConnection` - Database connection
- `ASPNETCORE_ENVIRONMENT` - Environment setting

### Docker Support (Optional)
Create `Dockerfile` for containerized deployment:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["EasyLearn.csproj", "."]
RUN dotnet restore "EasyLearn.csproj"
COPY . .
RUN dotnet build "EasyLearn.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EasyLearn.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EasyLearn.dll"]
```

## 🔄 Future Enhancements

- **Payment Integration**: Stripe/PayPal for paid courses
- **Email Notifications**: Course updates, certificates, reminders
- **Advanced Analytics**: Detailed learning analytics and reporting
- **Mobile App**: React Native or Flutter mobile application
- **Live Sessions**: Integration with video conferencing tools
- **Discussion Forums**: Course-specific discussion boards
- **Advanced Quizzes**: Timed quizzes, question banks, randomization
- **Bulk Operations**: Bulk user management, course imports
- **API Documentation**: Swagger/OpenAPI documentation
- **Caching**: Redis caching for improved performance

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation wiki

---

**EasyLearn** - Making online education accessible and engaging for everyone. 🎓✨
