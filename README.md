<h2>Reasoning Engine: Transparent and Auditable AI Reasoning with LLM Support</h1>

<h2>Overview</h2>
<p>
    An AI system designed to exceed human-level problem-solving in general contexts by: 

1. encoding knowledge in a human-readable graph,
2. performing explicit and auditable reasoning,
3. incorporating new information via data ingestion algorithms assisted by LLMs, and
4. enhancing knowledge accuracy and predictive power through refinement algorithms.
</p>

<h2>Features</h2>
<ul>

</ul>

<h2>Acknowledgments</h2>
<ul>
    <li><a href="https://github.com/conormckenzie/reasoning-engine">Original reasoning-engine repository</a></li>
</ul>

<p>For more information, feel free to reach out or open an issue in the repository.</p>

</body>
</html>

## Configuration
- **Environment Variables**: The application relies on the following environment variable, which should be set in the `.env` file:
   - `DATA_FOLDER_PATH`: The path to the main data directory where graph data and other files will be stored. Example:
     ```plaintext
     DATA_FOLDER_PATH=/path/to/your/data/folder
     ```
   - Ensure this environment variable is set before running the application.

## Getting Started
1. Ensure that the necessary environment variables are set in the `.env` file.
   - Example `.env` file:
     ```plaintext
     DATA_FOLDER_PATH=/path/to/your/data/folder
     ```
2. Build the project using the .NET CLI:
   \`\`\`bash
   dotnet build
   \`\`\`

3. Run the project:
   \`\`\`bash
   dotnet run --project reasoningEngine
   \`\`\`
# Reasoning Engine Project

## Status
This project is still in development and has no current releases.

## Getting Started

**Developers:**  
Welcome to the Reasoning Engine project! To get started with contributing to or reviewing the project, please check out the [`dev`](https://github.com/your-repo/repository-name/tree/dev) branch. 

## Contributing

If you wish to contribute to the project, please follow these steps:

1. **Clone the repository**:
   ```
   git clone https://github.com/your-repo/repository-name.git
   ```
2. **Checkout the `dev` branch**:
   ```
   git checkout dev
   ```
3. **Create a new branch for your changes**:
   ```
   git checkout -b your-feature-branch
   ```
4. **Make your changes and commit them**:
   ```
   git add .
   git commit -m "Your commit message"
   ```
5. **Push your changes**:
   ```
   git push origin your-feature-branch
   ```
6. **Create a pull request** on GitHub.

## Contact

For any questions or issues, please reach out to [your contact email or issue tracker link].

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgments

- Thanks to the contributors and open-source community for their support and tools.
