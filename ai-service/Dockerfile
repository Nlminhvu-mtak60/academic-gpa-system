FROM python:3.11-slim

WORKDIR /app

# Install dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy source code
COPY . .

EXPOSE 8000

ENV PYTHONUNBUFFERED=1
ENV AI_SERVICE_API_KEY=SuperSecretAiAdvisorApiKey123!

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
