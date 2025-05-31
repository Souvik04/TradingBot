FROM python:3.11-slim

WORKDIR /app

# Copy requirements and install first (better caching)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the full source code
COPY . .

ENV PYTHONUNBUFFERED=1

# Expose the port you're using in FastAPI (default 8000 unless you changed it)
EXPOSE 8000

# Healthcheck for Docker (expects /health endpoint to return 200)
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl --fail http://localhost:8000/health || exit 1

# Run FastAPI with uvicorn
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]