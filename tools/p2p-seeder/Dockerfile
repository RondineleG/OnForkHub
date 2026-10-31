FROM node:22-bookworm-slim
WORKDIR /app
COPY package.json ./
RUN npm install --omit=dev --no-audit --no-fund
COPY src ./src
RUN mkdir -p /media /metadata
ENV MEDIA_DIR=/media METADATA_DIR=/metadata
CMD ["npm", "start"]
