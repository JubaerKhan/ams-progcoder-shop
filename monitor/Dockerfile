# The monitor ran on the host on Docker Desktop, reached through
# host.docker.internal. On this Linux server it runs as a container on the
# same Docker network as the collector, so the collector reaches it by
# service name and the AMS Edge Agent sees a real container-to-container
# TCP connection instead of a host-gateway hop.
FROM python:3.12-slim

WORKDIR /app
COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY monitor.py alert-receiver.py ./

ENV MONITOR_HOST=0.0.0.0 \
    MONITOR_PORT=7787 \
    MONITOR_DATA_DIR=/data

EXPOSE 7787

# /health is the one route that stays open when MONITOR_API_KEY is set.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=5 \
  CMD python -c "import urllib.request,sys; sys.exit(0 if urllib.request.urlopen('http://127.0.0.1:7787/health',timeout=4).status==200 else 1)"

CMD ["python", "-u", "monitor.py"]
